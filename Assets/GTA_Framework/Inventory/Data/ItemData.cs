using GTAFramework.Inventory.Interfaces;
using UnityEngine;

namespace GTAFramework.Inventory.Data
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "GTA Framework/Item Data")]
    public class ItemData : ScriptableObject
    {
        public string itemName = "Item";
        public ItemType type;
        public Sprite icon;
        [Min(1)] public int maxStack = 1;
        public float effectValue = 10f;  // Cantidad de salud/armadura/munición

        public void ApplyEffect(IPickupReceiver receiver) => receiver.ApplyItemEffect(this);
    }
}