using System.Collections.Generic;
using UnityEngine;
using GTAFramework.Vehicle.Components;

namespace GTAFramework.Vehicle.Systems
{
    /// <summary>
    /// Registro de vehículos disponibles en la escena.
    /// Evita búsquedas costosas con FindObjectsByType.
    /// </summary>
    public class VehicleRegistry
    {
        private readonly List<VehicleController> _vehicles = new();

        public IReadOnlyList<VehicleController> Vehicles => _vehicles;

        public void Register(VehicleController vehicle)
        {
            if (!_vehicles.Contains(vehicle))
                _vehicles.Add(vehicle);
        }

        public void Unregister(VehicleController vehicle)
        {
            _vehicles.Remove(vehicle);
        }

        public VehicleController FindNearestAvailable(Vector3 position, float maxDistance)
        {
            VehicleController nearest = null;
            float nearestDist = maxDistance;

            foreach (var v in _vehicles)
            {
                if (v.IsOccupied || v.IsDestroyed) continue;

                float dist = Vector3.Distance(position, v.Transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = v;
                }
            }

            return nearest;
        }
    }
}