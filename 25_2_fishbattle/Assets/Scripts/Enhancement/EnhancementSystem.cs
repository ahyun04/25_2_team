using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

[System.Serializable]
public class EnhancementSystem
{
    public int FishClockHp { get; private set; }
    private readonly List<InventorySlot> _materialSlots;
    public IReadOnlyList<InventorySlot> MaterialSlots => _materialSlots;

    public UnityAction OnEnhancementStateChanged;

    public EnhancementSystem()
    {
        FishClockHp = 1; // 초기 HP 설정
        _materialSlots = new List<InventorySlot>(3);
        for (int i = 0; i < 3; i++)
        {
            _materialSlots.Add(new InventorySlot());
        }
    }

    // 특정 인덱스의 재료 슬롯에 아이템을 놓습니다.
    public void PlaceMaterial(int slotIndex, FishSO materialItem)
    {
        if (slotIndex < 0 || slotIndex >= _materialSlots.Count) return;

        // 이미 아이템이 있다면 먼저 인벤토리로 되돌림 (안전장치)
        if (!_materialSlots[slotIndex].IsEmpty)
        {
            InventoryHolder.Instance.InventorySystem.AddToInventory(_materialSlots[slotIndex].ItemData, 1);
        }

        _materialSlots[slotIndex].UpdateInventorySlot(materialItem, 1);
        InventoryHolder.Instance.InventorySystem.RemoveItem(materialItem, 1);

        OnEnhancementStateChanged?.Invoke();
    }

    // 특정 슬롯의 재료 하나를 인벤토리로 되돌립니다.
    public void ReturnMaterialFromSlot(int slotIndex)
    {
        // 인덱스 범위 확인
        if (slotIndex < 0 || slotIndex >= _materialSlots.Count) return;

        var slot = _materialSlots[slotIndex];
        if (!slot.IsEmpty)
        {
            // 1. 인벤토리에 아이템을 다시 추가
            InventoryHolder.Instance.InventorySystem.AddToInventory(slot.ItemData, 1);
            // 2. 이 데이터 슬롯을 비움
            slot.ClearSlot();
            // 3. UI를 업데이트하라고 신호를 보냄
            OnEnhancementStateChanged?.Invoke();
        }
    }

    // 모든 재료를 인벤토리로 되돌립니다.
    public void ReturnAllMaterialsToInventory()
    {
        foreach (var slot in _materialSlots)
        {
            if (!slot.IsEmpty)
            {
                InventoryHolder.Instance.InventorySystem.AddToInventory(slot.ItemData, 1);
            }
        }
        ClearAllMaterials();
    }

    // 강화를 시도합니다.
    public bool AttemptEnhancement()
    {
        if (_materialSlots.Count(slot => !slot.IsEmpty) < 3) return false;

        // 일단 1씩 늘어나게
        FishClockHp += 1;
        ClearAllMaterials();
        return true;
    }

    private void ClearAllMaterials()
    {
        foreach (var slot in _materialSlots)
        {
            slot.ClearSlot();
        }
        OnEnhancementStateChanged?.Invoke();
    }
}