using GTAFramework.Inventory.Data;
using UnityEngine;

namespace GTAFramework.Inventory.Interfaces
{
    public interface IPickupReceiver
    {
        bool CanReceiveItem(ItemData item);
        bool ReceiveItem(ItemData item, int quantity);
        void ApplyItemEffect(ItemData item);
    }
}