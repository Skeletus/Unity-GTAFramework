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

        // Reverse system
        private float _stoppedTimer = 0f;
        private bool _canReverse = false;

        // Debug
        public bool CanReverse => _canReverse;
        public float StoppedTimer => _stoppedTimer;

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
            float currentSpeed = _rb.linearVelocity.magnitude;

            // Detectar dirección del movimiento
            float forwardSpeed = Vector3.Dot(_rb.linearVelocity, _controller.Transform.forward);
            bool isMovingForward = forwardSpeed > 0.5f;
            bool isMovingBackward = forwardSpeed < -0.5f;
            bool isStopped = currentSpeed < 0.5f;

            // === LÓGICA DE REVERSA CON DELAY ===
            if (isStopped && MotorInput < -0.1f)
            {
                _stoppedTimer += Time.fixedDeltaTime;
                if (_stoppedTimer >= _data.reverseDelay)
                {
                    _canReverse = true;
                }
            }
            else if (MotorInput >= -0.1f)
            {
                _stoppedTimer = 0f;
                _canReverse = false;
            }

            // === ENGINE BRAKE ===
            float engineBrake = 0f;
            if (Mathf.Abs(MotorInput) < 0.1f && Mathf.Abs(BrakeInput) < 0.1f)
            {
                engineBrake = _data.engineBrakeTorque;
            }

            // === SPEED LIMITER ===
            float speedLimiter = 1f;
            if (currentSpeed >= _data.maxSpeed)
            {
                speedLimiter = 0f;
            }
            else if (currentSpeed > _data.maxSpeed * 0.9f)
            {
                speedLimiter = (_data.maxSpeed - currentSpeed) / (_data.maxSpeed * 0.1f);
            }

            // === CALCULAR TORQUE EFECTIVO ===
            float effectiveTorque = 0f;
            float effectiveBrake = _data.maxBrakeTorque * BrakeInput + engineBrake;

            if (MotorInput > 0.1f)
            {
                // Acelerar hacia adelante
                effectiveTorque = _data.maxMotorTorque * MotorInput * speedLimiter;
                _canReverse = false;
                _stoppedTimer = 0f;
            }
            else if (MotorInput < -0.1f)
            {
                if (isMovingForward && currentSpeed > 0.5f)
                {
                    // Frenando mientras avanza: aplicar freno fuerte
                    effectiveBrake += _data.maxBrakeTorque * Mathf.Abs(MotorInput);
                }
                else if (_canReverse)
                {
                    // Reversa activada después del delay
                    effectiveTorque = _data.maxMotorTorque * MotorInput * _data.reverseTorqueMultiplier;
                }
                // Si está detenido pero sin canReverse, no hace nada (espera el delay)
            }

            // === HANDBRAKE: Bloquear ruedas traseras ===
            float handbrakeTorque = Handbrake ? _data.maxBrakeTorque * 3f : 0f;

            foreach (var wheel in _controller.Wheels)
            {
                if (wheel.IsSteerable)
                    wheel.SteerAngle = CurrentSteerAngle;

                if (wheel.IsPowered)
                    wheel.MotorTorque = effectiveTorque;

                if (wheel.IsRear)
                {
                    wheel.BrakeTorque = effectiveBrake + handbrakeTorque;
                }
                else
                {
                    wheel.BrakeTorque = effectiveBrake;
                }
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