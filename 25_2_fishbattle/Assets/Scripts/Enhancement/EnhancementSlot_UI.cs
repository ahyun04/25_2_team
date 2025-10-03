using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class EnhancementSlot_UI : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    #region 레퍼런스
    [Header("UI 레퍼런스")]
    [SerializeField] private Image _itemImage;
    [SerializeField] private TextMeshProUGUI _itemNameText;

    public int SlotIndex { get; private set; }

    #endregion

    #region 초기화
    public void Initialize(int index)
    {
        SlotIndex = index;
    }

    #endregion

    #region 강화할 아이템 드롭
    // 드래그된 아이템이 이 슬롯 위에 드롭되었을 때 호출됩니다.
    public void OnDrop(PointerEventData eventData)
    {
        var sourceSlot = eventData.pointerDrag.GetComponent<InventorySlot_UI>();
        if (sourceSlot != null && sourceSlot.AssignedItem != null)
        {
            // 데이터 시스템에 아이템을 놓았다고 알림
            EnhancementHolder.Instance.EnhancementSystem.PlaceMaterial(SlotIndex, sourceSlot.AssignedItem);
        }
    }

    // 슬롯이 클릭되었을 때 호출됩니다.
    public void OnPointerClick(PointerEventData eventData)
    {
        // 만약 '우클릭'을 했고, 이 슬롯에 아이템이 있다면 (이미지가 보인다면)
        if (eventData.button == PointerEventData.InputButton.Right && _itemImage.sprite != null)
        {
            Debug.Log($"{SlotIndex}번 슬롯의 아이템을 우클릭으로 인벤토리에 반환합니다.");
            // 데이터 시스템에 이 슬롯의 아이템을 반환하라고 요청합니다.
            EnhancementHolder.Instance.EnhancementSystem.ReturnMaterialFromSlot(SlotIndex);
        }
    }

    // 이 메서드들은 이제 EnhancementManager가 호출하여 UI를 그림
    public void UpdateSlot(FishSO item)
    {
        _itemImage.sprite = item.Icon;
        _itemImage.color = Color.white;
        if (_itemNameText != null) _itemNameText.text = item.Name;
    }

    public void ClearSlot()
    {
        _itemImage.sprite = null;
        _itemImage.color = new Color(1, 1, 1, 0.5f);
        if (_itemNameText != null) _itemNameText.text = "";
    }

    #endregion
}