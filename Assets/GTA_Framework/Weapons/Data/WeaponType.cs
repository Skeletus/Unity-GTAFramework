using UnityEngine;

namespace GTAFramework.Weapons.Data
{
    /// <summary>
    /// Define una categoría/tipo de arma (Pistolas, Escopetas, Rifles, etc.)
    /// </summary>
    [CreateAssetMenu(fileName = "NewWeaponType", menuName = "GTA Framework/Weapon Type")]
    public class WeaponType : ScriptableObject
    {
        [Header("Info")]
        public string typeName = "Weapon Type";
        public Sprite categoryIcon;

        [Header("Settings")]
        [Tooltip("Máximo de armas que se pueden llevar de este tipo")]
        public int maxWeaponsPerSlot = 3;
    }
}