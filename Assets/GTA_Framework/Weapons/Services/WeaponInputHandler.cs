namespace GTAFramework.Weapons.Services
{
    using GTAFramework.Core.Services;

    public interface IWeaponInputHandler
    {
        bool IsPrevWeaponPressed { get; }
        bool IsNextWeaponPressed { get; }
        bool IsInteractPressed { get; }
        void ConsumeInteract();
    }

    public class WeaponInputHandler : IWeaponInputHandler
    {
        private readonly InputService _inputService;

        public WeaponInputHandler(InputService inputService)
        {
            _inputService = inputService;
        }

        public bool IsPrevWeaponPressed => _inputService.IsWeaponPrevPressed;
        public bool IsNextWeaponPressed => _inputService.IsWeaponNextPressed;
        public bool IsInteractPressed => _inputService.IsInteractPressed;

        public void ConsumeInteract() => _inputService.IsInteractPressed = false;
    }
}
