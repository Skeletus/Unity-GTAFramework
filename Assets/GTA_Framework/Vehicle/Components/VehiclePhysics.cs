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
            float targetAngle = _data.maxSteerAngle * SteerInput;
            CurrentSteerAngle = Mathf.Lerp(CurrentSteerAngle, targetAngle, _data.steerSpeed * Time.fixedDeltaTime);
        }

        private void ApplyWheelForces()
        {
            foreach (var wheel in _controller.Wheels)
            {
                if (wheel.IsSteerable)
                    wheel.SteerAngle = CurrentSteerAngle;

                if (wheel.IsPowered)
                    wheel.MotorTorque = _data.maxMotorTorque * MotorInput;

                wheel.BrakeTorque = _data.maxBrakeTorque * BrakeInput;
            }
        }
    }
}