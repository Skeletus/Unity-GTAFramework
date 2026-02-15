using System;
using UnityEngine;
using GTAFramework.Vehicle.Enums;

namespace GTAFramework.Vehicle.Interfaces
{
    /// <summary>
    /// Interfaz para el sistema de daño del vehículo.
    /// Permite desacoplar el controlador de la implementación de daño.
    /// </summary>
    public interface IVehicleDamage
    {
        // ========== STATE ==========
        float Health { get; }
        bool IsDestroyed { get; }

        // ========== EVENTS ==========
        event Action<float, DamageZone> OnDamage;
        event Action OnDestroy;

        // ========== METHODS ==========
        void ApplyDamage(float amount, DamageZone zone = DamageZone.General);
        void HandleCollision(Collision collision);
        void Repair(float amount);
        void RepairFully();
    }
}