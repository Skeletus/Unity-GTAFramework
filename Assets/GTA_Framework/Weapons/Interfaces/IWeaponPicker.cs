namespace GTAFramework.Weapons.Interfaces
{
    public interface IWeaponPicker
    {
        bool TryPickup(IWeaponInventory inventory);
        void RegisterPickup(Components.WeaponPickup pickup);
        void UnregisterPickup(Components.WeaponPickup pickup);
    }
}
