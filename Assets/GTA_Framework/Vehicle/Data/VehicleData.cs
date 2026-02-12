using GTAFramework.Vehicle.Enums;
using UnityEngine;

namespace GTAFramework.Vehicle.Data
{
    [CreateAssetMenu(fileName = "VehicleData", menuName = "GTA Framework/Vehicle/Data")]
    public class VehicleData : ScriptableObject
    {
        [Header("Identification")]
        public string vehicleName = "Vehicle";
        public VehicleType vehicleType = VehicleType.Car;

        [Header("Physics - Basic")]
        public float mass = 1500f;

        [Header("Wheels")]
        public float wheelRadius = 0.35f;
        public float suspensionDistance = 0.3f;
        public float suspensionSpring = 35000f;
        public float suspensionDamper = 4500f;
    }
}