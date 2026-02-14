using System;
using UnityEngine;
using GTAFramework.Vehicle.Data;
using GTAFramework.Vehicle.Enums;

namespace GTAFramework.Vehicle.Components
{
    /// <summary>
    /// Sistema de daño del vehículo.
    /// Maneja salud, aplicación de daño por colisiones, y deformación de mesh.
    /// </summary>
    public class VehicleDamage
    {
        private readonly VehicleController _controller;
        private readonly VehicleData _data;
        private readonly MeshFilter _bodyMesh;

        // Estado
        public float Health { get; private set; }
        public bool IsDestroyed => Health <= 0f;

        // Eventos
        public event Action<float, DamageZone> OnDamage;
        public event Action OnDestroy;

        public VehicleDamage(VehicleController controller, VehicleData data, MeshFilter bodyMesh = null)
        {
            _controller = controller;
            _data = data;
            _bodyMesh = bodyMesh;
            Health = data.maxHealth;
        }

        /// <summary>
        /// Aplica daño al vehículo.
        /// </summary>
        /// <param name="amount">Cantidad de daño.</param>
        /// <param name="zone">Zona afectada.</param>
        public void ApplyDamage(float amount, DamageZone zone = DamageZone.General)
        {
            if (IsDestroyed) return;

            Health -= amount;
            Health = Mathf.Max(Health, 0f);

            OnDamage?.Invoke(amount, zone);

            if (Health <= 0f)
            {
                OnDestroy?.Invoke();
            }
        }

        /// <summary>
        /// Procesa una colisión y aplica daño proporcional a la velocidad de impacto.
        /// </summary>
        /// <param name="collision">Datos de la colisión.</param>
        public void HandleCollision(Collision collision)
        {
            if (IsDestroyed) return;

            // Calcular fuerza de impacto
            float impactSpeed = collision.relativeVelocity.magnitude;

            // Calcular daño basado en velocidad y multiplicador
            float damage = impactSpeed * _data.crashDamageMultiplier;

            // Determinar zona de daño
            DamageZone zone = DetermineDamageZone(collision);

            // Aplicar daño
            ApplyDamage(damage, zone);

            // Aplicar deformación visual (opcional)
            if (_data.deformationStrength > 0f)
            {
                ApplyDeformation(collision, damage);
            }
        }

        /// <summary>
        /// Determina la zona de daño basándose en la normal del impacto.
        /// </summary>
        private DamageZone DetermineDamageZone(Collision collision)
        {
            if (collision.contacts.Length == 0) return DamageZone.General;

            Vector3 impactNormal = collision.contacts[0].normal;
            Vector3 localNormal = _controller.transform.InverseTransformDirection(impactNormal);

            // Determinar zona basándose en la dirección del impacto
            if (localNormal.z > 0.5f) return DamageZone.Front;
            if (localNormal.z < -0.5f) return DamageZone.Rear;
            if (localNormal.x > 0.5f) return DamageZone.Right;
            if (localNormal.x < -0.5f) return DamageZone.Left;
            if (localNormal.y > 0.5f) return DamageZone.Top;
            if (localNormal.y < -0.5f) return DamageZone.Bottom;

            return DamageZone.General;
        }

        /// <summary>
        /// Aplica deformación visual a los meshes del vehículo.
        /// </summary>
        private void ApplyDeformation(Collision collision, float damage)
        {
            // Usar el mesh asignado o buscar en hijos
            MeshFilter meshToDeform = _bodyMesh;

            if (meshToDeform == null)
            {
                // Fallback: buscar en hijos
                meshToDeform = _controller.GetComponentInChildren<MeshFilter>();
            }

            if (meshToDeform == null || meshToDeform.sharedMesh == null)
            {
                Debug.LogWarning($"[VehicleDamage] No body mesh found on {_controller.name}");
                return;
            }

            if (!meshToDeform.mesh.isReadable)
            {
                Debug.LogWarning($"[VehicleDamage] Mesh '{meshToDeform.name}' is not readable. Enable Read/Write in import settings.");
                return;
            }

            Vector3 impactPoint = collision.contacts[0].point;
            Vector3 impactDirection = collision.relativeVelocity.normalized;
            float deformationRadius = 1.5f;

            Vector3[] vertices = meshToDeform.mesh.vertices;
            bool modified = false;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 worldVertex = meshToDeform.transform.TransformPoint(vertices[i]);
                float distance = Vector3.Distance(worldVertex, impactPoint);

                if (distance < deformationRadius)
                {
                    float deformAmount = (1f - distance / deformationRadius) * damage * _data.deformationStrength * 0.01f;
                    vertices[i] += meshToDeform.transform.InverseTransformDirection(impactDirection) * deformAmount;
                    modified = true;
                }
            }

            if (modified)
            {
                meshToDeform.mesh.vertices = vertices;
                meshToDeform.mesh.RecalculateNormals();
                meshToDeform.mesh.RecalculateBounds();

                Debug.Log($"[VehicleDamage] Deformed mesh on {_controller.name}");
            }
        }

        /// <summary>
        /// Repara el vehículo (para futuro sistema de reparación).
        /// </summary>
        /// <param name="amount">Cantidad de salud a restaurar.</param>
        public void Repair(float amount)
        {
            if (_data == null) return;

            Health = Mathf.Min(Health + amount, _data.maxHealth);
        }

        /// <summary>
        /// Restaura la salud al máximo.
        /// </summary>
        public void RepairFully()
        {
            if (_data == null) return;

            Health = _data.maxHealth;
        }
    }
}