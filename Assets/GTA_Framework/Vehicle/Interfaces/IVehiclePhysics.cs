using UnityEngine;

namespace GTAFramework.Vehicle.Interfaces
{
    /// <summary>
    /// Interfaz para el sistema de física del vehículo.
    /// Permite desacoplar el controlador de la implementación de física.
    /// </summary>
    public interface IVehiclePhysics
    {
        // ========== INPUTS ==========
        float MotorInput { get; set; }
        float SteerInput { get; set; }
        float BrakeInput { get; set; }
        bool Handbrake { get; set; }

        // ========== OUTPUTS ==========
        float CurrentSteerAngle { get; }

        // ========== METHODS ==========
        void FixedUpdate();
        void ResetInputs();
    }
}