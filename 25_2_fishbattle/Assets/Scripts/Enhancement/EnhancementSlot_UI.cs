using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class EnhancementSlot_UI : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    #region 레퍼런스
    [Header("UI 레퍼런스")]
    [SerializeField] private UIModelViewer _modelViewer;
    [SerializeField] private TextMeshProUGUI _itemNameText;

    public int SlotIndex { get; private set; }
    private FishSO _currentItem;

    #endregion

    #region 초기화
    public void Initialize(int index)
    {
        SlotIndex = index;
    }
    #endregion

    #region 인벤토리에서 재료 가져오기
    public void OnDrop(PointerEventData eventData)
    {
        if (_currentItem != null) return;

        var sourceSlot = eventData.pointerDrag?.GetComponent<InventorySlot_UI>();

        if (sourceSlot != null && sourceSlot.AssignedItem != null)
        {
            FishSO itemToMove = sourceSlot.AssignedItem;

            if (InventoryHolder.Instance != null)
            {
                InventoryHolder.Instance.InventorySystem.RemoveItem(itemToMove, 1);
            }

            EnhancementHolder.Instance.EnhancementSystem.PlaceMaterial(SlotIndex, itemToMove);
        }
    }
    #endregion

    #region 재료 반환하기
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && _currentItem != null)
        {
            if (InventoryHolder.Instance != null)
            {
                bool success = InventoryHolder.Instance.InventorySystem.AddToInventory(_currentItem, 1);

                if (!success)
                {
                    Debug.LogWarning("인벤토리가 가득 차서 재료를 뺄 수 없습니다!");
                    return;
                }
            }

            EnhancementHolder.Instance.EnhancementSystem.ClearSlot(SlotIndex);
        }
    }
    #endregion

    #region UI 갱신 (Manager에 의해 호출됨)
    public void UpdateSlot(FishSO item)
    {
        _currentItem = item;

        if (item != null)
        {
            _modelViewer.ShowEnchancementModel(item.Prefab);
            if (_itemNameText != null) _itemNameText.text = item.Name;
        }
        else
        {
            ClearSlotUI();
        }
    }

    public void ClearSlot()
    {
        ClearSlotUI();
    }

    private void ClearSlotUI()
    {
        _currentItem = null;
        _modelViewer.ClearModel();
        if (_itemNameText != null) _itemNameText.text = "";
    }
    #endregion
}