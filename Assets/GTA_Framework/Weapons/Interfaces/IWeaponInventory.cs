namespace GTAFramework.Weapons.Interfaces
{
    using GTAFramework.Weapons.Data;
    using System.Collections.Generic;
    using System;

    public interface IWeaponInventory
    {
        WeaponData CurrentWeapon { get; }
        IReadOnlyList<WeaponType> WeaponOrder { get; }
        bool HasWeapons { get; }

        bool HasWeaponType(WeaponType type);
        bool TryAddOrReplace(WeaponData weaponData);
        bool EquipByType(WeaponType type);
        void EquipNext();
        void EquipPrevious();

        event Action<WeaponData> OnWeaponEquipped;
    }
}
