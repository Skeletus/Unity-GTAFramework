using GTAFramework.Vehicle.Interfaces;
using System;
using UnityEngine;

namespace GTAFramework.Vehicle.Components.VehicleCollision
{
    public class VehicleCollisionHandler : IVehicleCollisionHandler
    {
        private readonly IVehicleDamage _damage;
        private readonly Func<bool> _isDestroyed;
        private readonly float _impactThreshold;

        public VehicleCollisionHandler(IVehicleDamage damage, Func<bool> isDestroyed, float impactThreshold = 5f)
        {
            _damage = damage;
            _isDestroyed = isDestroyed;
            _impactThreshold = impactThreshold;
        }

        public void HandleCollision(Collision collision)
        {
            if (_isDestroyed()) return;

            float impactForce = collision.relativeVelocity.magnitude;
            if (impactForce > _impactThreshold)
            {
                _damage?.HandleCollision(collision);
            }
        }
    }
}
