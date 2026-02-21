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

        [Header("Firearm Settings")]
        [Tooltip("Si esta activo, esta arma puede apuntar y disparar.")]
        public bool isFirearm = true;

        [Tooltip("Dano por disparo (hitscan).")]
        [Min(0f)] public float damage = 20f;

        [Tooltip("Disparos por segundo.")]
        [Min(0.1f)] public float fireRate = 6f;

        [Tooltip("Rango maximo del raycast.")]
        [Min(1f)] public float range = 60f;

        [Tooltip("Multiplicador de velocidad al apuntar.")]
        [Range(0.1f, 1f)] public float aimMoveSpeedMultiplier = 0.6f;

        [Header("Equip Pose (Local)")]
        [Tooltip("Si esta activado, se aplican estos valores al instanciar el arma en el weaponHolder.")]
        public bool useCustomPose = false;
        public Vector3 localPosition = Vector3.zero;
        public Vector3 localEulerAngles = Vector3.zero;
        public Vector3 localScale = Vector3.one;
    }
}
