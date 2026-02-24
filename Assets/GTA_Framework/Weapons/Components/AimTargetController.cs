using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using GTAFramework.GTACamera.Components;

namespace GTAFramework.Weapons.Components
{
    /// <summary>
    /// Mueve el aim target hacia donde apunta el mouse (ScreenPointToRay).
    /// Usa raycast para colocar la esfera en el punto de impacto, o a una distancia fija.
    /// </summary>
    [DisallowMultipleComponent]
    public class AimTargetController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _aimTarget;
        [SerializeField] private Transform _cameraTransform;

        [Header("Input")]
        [Tooltip("Si esta activo, usa la posicion real del mouse en pantalla.")]
        [SerializeField] private bool _useMouseScreenPosition = true;

        [Tooltip("Si el mouse no esta disponible, usa el centro de la pantalla.")]
        [SerializeField] private bool _fallbackToScreenCenter = true;

        [Header("Ignore")]
        [Tooltip("Raiz del jugador para ignorar su propio collider en el raycast.")]
        [SerializeField] private Transform _ignoreRoot;

        [Header("Distance")]
        [SerializeField, Min(1f)] private float _maxDistance = 60f;
        [SerializeField, Min(0.5f)] private float _defaultDistance = 25f;

        [Header("Raycast")]
        [SerializeField] private LayerMask _aimMask = ~0;
        [SerializeField] private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore;

        [Header("Smoothing")]
        [SerializeField] private bool _useSmoothing = true;
        [SerializeField, Min(0f)] private float _smoothTime = 0.05f;

        private Vector3 _smoothVelocity;
        private bool _warnedMissingCamera;
        private static readonly RaycastHit[] _hits = new RaycastHit[16];

        private void Awake()
        {
            if (_aimTarget == null)
                _aimTarget = transform;
        }

        private void LateUpdate()
        {
            if (_aimTarget == null)
                return;

            ResolveCamera();
            if (_cameraTransform == null)
                return;

            Ray ray = BuildAimRay();
            Vector3 desiredPos = GetDesiredAimPosition(ray);

            if (_useSmoothing)
            {
                _aimTarget.position = Vector3.SmoothDamp(
                    _aimTarget.position,
                    desiredPos,
                    ref _smoothVelocity,
                    _smoothTime
                );
            }
            else
            {
                _aimTarget.position = desiredPos;
            }
        }

        private void ResolveCamera()
        {
            if (_cameraTransform != null)
                return;

            if (Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
                return;
            }

            var thirdPersonCamera = FindFirstObjectByType<ThirdPersonCamera>();
            if (thirdPersonCamera != null)
            {
                _cameraTransform = thirdPersonCamera.transform;
                return;
            }

            if (!_warnedMissingCamera)
            {
                _warnedMissingCamera = true;
                Debug.LogWarning("[AimTargetController] No se encontro una camara. Asigna Camera Transform o etiqueta la camara como MainCamera.");
            }
        }

        private Ray BuildAimRay()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                // Fallback: ray desde transform de camara
                return new Ray(_cameraTransform.position, _cameraTransform.forward);
            }

            if (_useMouseScreenPosition && Mouse.current != null)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                return cam.ScreenPointToRay(mousePos);
            }

            if (_fallbackToScreenCenter)
            {
                Vector3 center = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
                return cam.ScreenPointToRay(center);
            }

            return new Ray(_cameraTransform.position, _cameraTransform.forward);
        }

        private Vector3 GetDesiredAimPosition(Ray ray)
        {
            float maxDist = Mathf.Max(_defaultDistance, _maxDistance);

            int hitCount = Physics.RaycastNonAlloc(ray, _hits, maxDist, _aimMask, _triggerInteraction);
            if (hitCount > 0)
            {
                Array.Sort(_hits, 0, hitCount, new RaycastHitDistanceComparer());

                for (int i = 0; i < hitCount; i++)
                {
                    if (IsIgnored(_hits[i].collider))
                        continue;

                    return _hits[i].point;
                }
            }

            float distance = Mathf.Min(_defaultDistance, maxDist);
            return ray.origin + ray.direction * distance;
        }

        private bool IsIgnored(Collider col)
        {
            if (col == null || _ignoreRoot == null)
                return false;

            return col.transform.IsChildOf(_ignoreRoot);
        }

        private sealed class RaycastHitDistanceComparer : IComparer<RaycastHit>
        {
            public int Compare(RaycastHit x, RaycastHit y)
            {
                return x.distance.CompareTo(y.distance);
            }
        }
    }
}
