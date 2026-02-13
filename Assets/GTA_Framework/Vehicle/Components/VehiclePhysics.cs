using UnityEngine;
using GTAFramework.Vehicle.Data;

namespace GTAFramework.Vehicle.Components
{
    public class VehiclePhysics
    {
        private readonly VehicleController _controller;
        private readonly Rigidbody _rb;
        private readonly VehicleData _data;

        // Input
        public float MotorInput { get; set; }
        public float SteerInput { get; set; }
        public float BrakeInput { get; set; }
        public bool Handbrake { get; set; }

        // Output
        public float CurrentSteerAngle { get; private set; }

        public VehiclePhysics(VehicleController controller, Rigidbody rb, VehicleData data)
        {
            _controller = controller;
            _rb = rb;
            _data = data;
        }

        public void FixedUpdate()
        {
            CalculateSteering();
            ApplyWheelForces();
        }

        private void CalculateSteering()
        {
            // Obtener velocidad actual del vehículo
            float currentSpeed = _rb.linearVelocity.magnitude;

            // Calcular factor de reducción basado en velocidad
            // Normalizamos la velocidad respecto al threshold
            float normalizedSpeed = Mathf.Clamp01(currentSpeed / _data.speedThreshold);

            // Usar la curva para obtener el factor (1, 0.3 = alta velocidad)
            float speedFactor = _data.steerReductionCurve.Evaluate(normalizedSpeed);

            // Aplicar factor al ángulo máximo
            float effectiveMaxAngle = _data.maxSteerAngle * speedFactor;

            // Calcular ángulo objetivo
            float targetAngle = effectiveMaxAngle * SteerInput;

            // Suavizar transición
            CurrentSteerAngle = Mathf.Lerp(CurrentSteerAngle, targetAngle, _data.steerSpeed * Time.fixedDeltaTime);
        }

        private void ApplyWheelForces()
        {
            float engineBrake = 0f;
            if (Mathf.Abs(MotorInput) < 0.1f && Mathf.Abs(BrakeInput) < 0.1f)
            {
                engineBrake = _data.engineBrakeTorque; // Nuevo campo en VehicleData
            }

            foreach (var wheel in _controller.Wheels)
            {
                if (wheel.IsSteerable)
                    wheel.SteerAngle = CurrentSteerAngle;

                if (wheel.IsPowered)
                    wheel.MotorTorque = _data.maxMotorTorque * MotorInput;

                wheel.BrakeTorque = _data.maxBrakeTorque * BrakeInput + engineBrake;
            }
        }

        public void ResetInputs()
        {
            MotorInput = 0f;
            SteerInput = 0f;
            BrakeInput = 0f;
            Handbrake = false;
        }
    }
}