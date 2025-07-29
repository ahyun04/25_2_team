using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUIController : MonoBehaviour
{
    #region 레퍼런스
    [SerializeField] private InventoryHolder _inventoryHolder;
    [SerializeField] private List<InventorySlot_UI> _slotUIs;

    #endregion

    #region 초기화
    void Start()
    {
        if (_inventoryHolder == null) return;

        _inventoryHolder.gameObject.SetActive(false);
        _inventoryHolder.InventorySystem.OnInventorySlotChanged += UpdateSlotUI;

        UpdateAllSlots();
    }

    #endregion

    #region 이벤트 구독
    private void OnEnable()
    {
        if (_inventoryHolder != null)
        {
            _inventoryHolder.InventorySystem.OnInventorySlotChanged += UpdateSlotUI;
            UpdateAllSlots();
        }
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
