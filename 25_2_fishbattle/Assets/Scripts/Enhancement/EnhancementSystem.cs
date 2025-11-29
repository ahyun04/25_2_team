using System;
using System.Collections.Generic;
using UnityEngine;

public class EnhancementSystem
{
    public class EnhancementSlotData
    {
        public FishSO ItemData;
        public bool IsEmpty => ItemData == null;
    }

    public List<EnhancementSlotData> MaterialSlots { get; private set; }
    public event Action OnEnhancementStateChanged;

    public EnhancementSystem()
    {
        MaterialSlots = new List<EnhancementSlotData>();
        for (int i = 0; i < 3; i++) MaterialSlots.Add(new EnhancementSlotData());
    }

    // 슬롯에 아이템 데이터 등록
    public void PlaceMaterial(int slotIndex, FishSO item)
    {
        MaterialSlots[slotIndex].ItemData = item;
        OnEnhancementStateChanged?.Invoke();
    }

    // 슬롯 비우기
    public void ClearSlot(int slotIndex)
    {
        MaterialSlots[slotIndex].ItemData = null;
        OnEnhancementStateChanged?.Invoke();
    }

    // 유효성 검사
    public string ValidateEnhancement()
    {
        foreach (var slot in MaterialSlots)
        {
            if (slot.IsEmpty) return "재료가 부족합니다.";
        }

        int firstId = MaterialSlots[0].ItemData.FishId;
        if (MaterialSlots[1].ItemData.FishId != firstId ||
            MaterialSlots[2].ItemData.FishId != firstId)
        {
            return "재료가 모두 같아야 합니다.";
        }

        return string.Empty;
    }

    // 강화 시도
    public FishSO AttemptEnhancement()
    {
        if (ValidateEnhancement() != string.Empty) return null;

        FishSO enhancedFish = MaterialSlots[0].ItemData.CreateEnhancedInstance();

        foreach (var slot in MaterialSlots)
        {
            slot.ItemData = null;
        }

        OnEnhancementStateChanged?.Invoke();
        return enhancedFish;
    }
}