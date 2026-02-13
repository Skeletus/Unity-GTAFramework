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

        [Header("Driver seat")]
        public Transform driverSeatOffset;

        [Header("Engine")]
        public float maxMotorTorque = 400f;
        public float maxBrakeTorque = 300f;
        public float engineBrakeTorque = 50f;
        public float maxSpeed = 30f; // m/s (= 108 km/h)

        [Header("Reverse")]
        public float reverseDelay = 1.5f; // Segundos de espera antes de reversar
        public float reverseTorqueMultiplier = 0.6f; // Reversa más lenta que avance

        [Header("Steering")]
        public float maxSteerAngle = 35f;

        [Header("Sensitive Steering")]
        public float steerSpeed = 10f;

        [Tooltip("Velocidad a partir de la cual se reduce el ángulo de dirección")]
        public float speedThreshold = 10f; // m/s (~36 km/h)

        [Tooltip("Factor de reducción máxima del ángulo a alta velocidad")]
        [Range(0.1f, 0.5f)] public float highSpeedSteerFactor = 0.3f;

        [Tooltip("Curva de reducción de steering según velocidad")]
        public AnimationCurve steerReductionCurve = AnimationCurve.EaseInOut(0, 1, 1, 0.3f);
    }
}