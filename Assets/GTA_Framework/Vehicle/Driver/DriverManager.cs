using GTAFramework.Vehicle.Interfaces;
using System;
using UnityEngine;

namespace GTAFramework.Vehicle.VehicleDriverManager
{
    /// <summary>
    /// Gestiona la entrada y salida de conductores del vehículo.
    /// </summary>
    public class DriverManager : IDriverManager
    {
        private readonly IVehicle _vehicle;
        private readonly Transform _driverSeat;
        private readonly Func<bool> _canEnter;

        public IDriver CurrentDriver { get; private set; }
        public bool IsOccupied => CurrentDriver != null;

        /// <summary>
        /// Crea una nueva instancia de DriverManager.
        /// </summary>
        /// <param name="vehicle">El vehículo que este manager gestiona.</param>
        /// <param name="driverSeat">Transform del asiento del conductor.</param>
        /// <param name="canEnter">Función que determina si se puede entrar al vehículo.</param>
        public DriverManager(IVehicle vehicle, Transform driverSeat, Func<bool> canEnter)
        {
            _vehicle = vehicle;
            _driverSeat = driverSeat;
            _canEnter = canEnter;
        }

        public void Enter(IDriver driver)
        {
            if (IsOccupied || !_canEnter()) return;

            CurrentDriver = driver;
            driver.OnVehicleEnter(_vehicle);

            if (_driverSeat != null)
            {
                driver.Transform.position = _driverSeat.position;
                driver.Transform.rotation = _driverSeat.rotation;
                driver.Transform.SetParent(_driverSeat);
            }
        }

        public void Exit()
        {
            if (!IsOccupied) return;

            var exitPos = _vehicle.Transform.position + _vehicle.Transform.right * 2f;
            CurrentDriver.Transform.SetParent(null);
            CurrentDriver.Transform.position = exitPos;
            CurrentDriver.OnVehicleExit(_vehicle);
            CurrentDriver = null;
        }
    }
}
