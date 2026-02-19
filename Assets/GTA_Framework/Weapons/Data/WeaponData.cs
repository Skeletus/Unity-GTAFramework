using UnityEngine;

namespace GTAFramework.Weapons.Data
{
    /// <summary>
    /// Define un arma específica (9mm, Desert Eagle, AK-47, etc.)
    /// </summary>
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "GTA Framework/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [Header("Info")]
        public string weaponName = "Weapon";
        public WeaponType weaponType;
        public Sprite icon;

        [Header("Stats")]
        public float damage = 10f;
        public float fireRate = 0.5f;
        public int magazineSize = 12;
        public float reloadTime = 2f;
        public float range = 50f;

        [Header("Visual/Audio")]
        public GameObject weaponPrefab;
        public AudioClip fireSound;
        public AudioClip reloadSound;
    }
}