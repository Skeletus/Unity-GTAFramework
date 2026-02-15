using UnityEngine;
using GTAFramework.Vehicle.Data;
using GTAFramework.Vehicle.Interfaces;
using GTAFramework.Vehicle.Components.Wheels;

namespace GTAFramework.Vehicle.Components
{
    public class VehiclePhysics : IVehiclePhysics
    {
        private readonly VehicleController _controller;
        private readonly Rigidbody _rb;
        private readonly VehicleData _data;
        private readonly WheelController[] _wheels;

        // Input
        public float MotorInput { get; set; }
        public float SteerInput { get; set; }
        public float BrakeInput { get; set; }
        public bool Handbrake { get; set; }

        // Output
        public float CurrentSteerAngle { get; private set; }

        // Reverse system
        private float _stoppedTimer = 0f;
        private bool _canReverse = false;

        // Debug
        public bool CanReverse => _canReverse;
        public float StoppedTimer => _stoppedTimer;

        public VehiclePhysics(VehicleController controller, Rigidbody rb, VehicleData data, WheelController[] wheels)
        {
            _controller = controller;
            _rb = rb;
            _wheels = wheels;
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
            // 1. Detectar estado del movimiento
            var (isMovingForward, isMovingBackward, isStopped) = CalculateMovementState();

            // 2. Actualizar lógica de reversa
            UpdateReverseLogic(isStopped);

            // 3. Calcular valores auxiliares
            float engineBrake = CalculateEngineBrake();
            float speedLimiter = CalculateSpeedLimiter();

            // 4. Calcular torque y freno efectivo
            var (effectiveTorque, effectiveBrake) = CalculateEffectiveTorque(
                isMovingForward, isMovingBackward, speedLimiter, engineBrake);

            // 5. Aplicar a las ruedas
            ApplyToWheels(effectiveTorque, effectiveBrake);
        }

        private (bool isMovingForward, bool isMovingBackward, bool isStopped) CalculateMovementState()
        {
            float currentSpeed = _rb.linearVelocity.magnitude;
            float forwardSpeed = Vector3.Dot(_rb.linearVelocity, _controller.Transform.forward);

            return (
                isMovingForward: forwardSpeed > _data.DIRECTION_THRESHOLD,
                isMovingBackward: forwardSpeed < -_data.DIRECTION_THRESHOLD,
                isStopped: currentSpeed < _data.DIRECTION_THRESHOLD
            );
        }

        private void UpdateReverseLogic(bool isStopped)
        {
            if (isStopped && MotorInput < -_data.INPUT_DEADZONE)
            {
                _stoppedTimer += Time.fixedDeltaTime;
                if (_stoppedTimer >= _data.reverseDelay)
                {
                    _canReverse = true;
                }
            }
            else if (MotorInput >= -_data.INPUT_DEADZONE)
            {
                _stoppedTimer = 0f;
                _canReverse = false;
            }
        }

        private float CalculateEngineBrake()
        {
            if (Mathf.Abs(MotorInput) < _data.INPUT_DEADZONE && Mathf.Abs(BrakeInput) < _data.INPUT_DEADZONE)
            {
                return _data.engineBrakeTorque;
            }
            return 0f;
        }

        private float CalculateSpeedLimiter()
        {
            float currentSpeed = _rb.linearVelocity.magnitude;

            if (currentSpeed >= _data.maxSpeed)
            {
                return 0f;
            }
            if (currentSpeed > _data.maxSpeed * 0.9f)
            {
                return (_data.maxSpeed - currentSpeed) / (_data.maxSpeed * 0.1f);
            }
            return 1f;
        }

        private (float torque, float brake) CalculateEffectiveTorque(
            bool isMovingForward,
            bool isMovingBackward,
            float speedLimiter,
            float engineBrake)
        {
            float currentSpeed = _rb.linearVelocity.magnitude;
            float effectiveTorque = 0f;
            float effectiveBrake = engineBrake;

            if (MotorInput > _data.INPUT_DEADZONE)
            {
                // Si va hacia atrás, frenar primero (comportamiento simétrico)
                if (isMovingBackward && currentSpeed > _data.SPEED_THRESHOLD)
                {
                    effectiveBrake += _data.maxBrakeTorque * MotorInput;
                }
                else
                {
                    effectiveTorque = _data.maxMotorTorque * MotorInput * speedLimiter;
                    _canReverse = false;
                    _stoppedTimer = 0f;
                }
            }
            else if (MotorInput < -_data.INPUT_DEADZONE)
            {
                if (isMovingForward && BrakeInput > _data.SPEED_THRESHOLD)
                {
                    effectiveBrake += _data.maxBrakeTorque * BrakeInput;
                }
                else if (_canReverse)
                {
                    effectiveTorque = _data.maxMotorTorque * MotorInput * _data.reverseTorqueMultiplier;
                }
            }

            return (effectiveTorque, effectiveBrake);
        }

        private void ApplyToWheels(float effectiveTorque, float effectiveBrake)
        {
            float handbrakeTorque = Handbrake ? _data.maxBrakeTorque * _data.HANDBRAKE_MULTIPLIER : 0f;

            foreach (var wheel in _wheels)
            {
                if (wheel.IsSteerable)
                    wheel.SteerAngle = CurrentSteerAngle;

                if (wheel.IsPowered)
                    wheel.MotorTorque = effectiveTorque;

                wheel.BrakeTorque = wheel.IsRear
                    ? effectiveBrake + handbrakeTorque
                    : effectiveBrake;
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