using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUIController : MonoBehaviour
{
    #region 레퍼런스
    [SerializeField] private InventoryHolder _inventoryHolder;
    [SerializeField] private List<InventorySlot_UI> _slotUIs;

    #endregion

    #region 이벤트 구독
    private void OnEnable()
    {
        if (_inventoryHolder == null)
        {
            _inventoryHolder = FindObjectOfType<InventoryHolder>();
            if (_inventoryHolder == null)
            {
                Debug.LogError("씬에 InventoryHolder가 없습니다!");
                return;
            }
        }

        // 이벤트 구독 전에 혹시 모를 이전 구독을 제거하고 새로 구독합니다. (더 안전함)
        _inventoryHolder.InventorySystem.OnInventorySlotChanged -= UpdateSlotUI;
        _inventoryHolder.InventorySystem.OnInventorySlotChanged += UpdateSlotUI;

        // 활성화되는 즉시 UI를 한번 새로고침합니다.
        UpdateAllSlots();
    }

    private void OnDisable()
    {
        if (_inventoryHolder != null)
        {
            _inventoryHolder.InventorySystem.OnInventorySlotChanged -= UpdateSlotUI;
        }
    }

    #endregion

    #region 인벤토리 슬롯 관리
    private void UpdateSlotUI(InventorySlot updatedSlot)
    {
        UpdateAllSlots();
    }

    private void UpdateAllSlots()
    {
        var _inventorySlots = _inventoryHolder.InventorySystem.InventorySlots;

        for (int i = 0; i < _slotUIs.Count; i++)
        {
            var ui = _slotUIs[i];

            if (i < _inventorySlots.Count)
            {
                var slot = _inventorySlots[i];
                ui.UpdateSlot(slot.ItemData, slot.StackSize);
            }
            else
            {
                ui.UpdateSlot(null, 0);
            }
        }
    }

    #endregion
}
