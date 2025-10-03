using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    #region 레퍼런스
    [SerializeField] private FishSO _itemData;
    [SerializeField] private int _stackSize;

    public FishSO ItemData => _itemData;
    public int StackSize => _stackSize;

    public bool IsEmpty => ItemData == null || StackSize <= 0;
    public bool IsFull => !IsEmpty && StackSize >= ItemData.MaxStackSize;

    #endregion

    #region 인벤토리 슬롯
    public InventorySlot(FishSO source, int amount)
    {
        _itemData = source;
        _stackSize = amount;
    }

    public InventorySlot()
    {
        ClearSlot();
    }

    public void ClearSlot()
    {
        _itemData = null;
        _stackSize = -1;
    }

    public void UpdateInventorySlot(FishSO data, int amount)
    {
        _itemData = data;
        _stackSize = amount;
    }

    public void AddToStack(int amount)
    {
        _stackSize += amount;
    }

    public void RemoveFromToStack(int amount)
    {
        _stackSize -= amount;
    }

    #endregion
}
