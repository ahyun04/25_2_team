using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

[System.Serializable]
public class InventorySystem
{
    #region 레퍼런스
    [SerializeField] private List<InventorySlot> _inventorySlots;

    public List<InventorySlot> InventorySlots => _inventorySlots;
    public int InventorySize => _inventorySlots.Count;

    public UnityAction<InventorySlot> OnInventorySlotChanged;

    public InventorySystem(int size)
    {
        _inventorySlots = new List<InventorySlot>(size);

        for (int i = 0; i < size; i++)
        {
            _inventorySlots.Add(new InventorySlot());
        }
    }

    #endregion

    #region 아이템 제거 및 정리
    public void RemoveItem(FishSO fish, int amount)
    {
        int remainAmount = amount;

        for (int i = 0; i < _inventorySlots.Count; i++)
        {
            var slot = _inventorySlots[i];
            if (slot.ItemData == fish)
            {
                int removeCount = Mathf.Min(remainAmount, slot.StackSize);
                slot.RemoveFromToStack(removeCount);
                remainAmount -= removeCount;

                if (slot.StackSize <= 0)
                {
                    slot.ClearSlot();
                }

                OnInventorySlotChanged?.Invoke(slot);

                if (remainAmount <= 0)
                    break;
            }
        }

        CompactInventory();
        OnInventorySlotChanged?.Invoke(null);
    }

    public void CompactInventory()
    {
        int writeIndex = 0;

        for (int readIndex = 0; readIndex < _inventorySlots.Count; readIndex++)
        {
            if (!_inventorySlots[readIndex].IsEmpty)
            {
                if (writeIndex != readIndex)
                {
                    _inventorySlots[writeIndex] = _inventorySlots[readIndex];
                    _inventorySlots[readIndex] = new InventorySlot();

                    OnInventorySlotChanged?.Invoke(_inventorySlots[writeIndex]);
                    OnInventorySlotChanged?.Invoke(_inventorySlots[readIndex]);
                }
                writeIndex++;
            }
        }
    }

    #endregion
}
