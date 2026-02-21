using UnityEngine;

namespace GTAFramework.Weapons.Data
{
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "GTA Framework/Weapons/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [Header("Info")]
        public string weaponName = "Weapon";
        public WeaponType type = WeaponType.Pistol;

        [Header("Visuals")]
        public GameObject weaponPrefab;

        [Header("Equip Pose (Local)")]
        [Tooltip("Si está activado, se aplican estos valores al instanciar el arma en el weaponHolder.")]
        public bool useCustomPose = false;
        public Vector3 localPosition = Vector3.zero;
        public Vector3 localEulerAngles = Vector3.zero;
        public Vector3 localScale = Vector3.one;
    }
}
