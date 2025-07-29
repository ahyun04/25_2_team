using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot_UI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    #region 레퍼런스
    [SerializeField] private Image _itemImage;
    [SerializeField] private TextMeshProUGUI _itemCountText;
    [SerializeField] private TextMeshProUGUI _itemNameText;

    public FishSO AssignedItem { get; private set; }
    public int StackSize { get; private set; }

    #endregion

    #region 업데이트 슬롯
    public void UpdateSlot(FishSO item, int _stackSize)
    {
        AssignedItem = item;
        StackSize = _stackSize;

        if (item != null)
        {
            _itemImage.sprite = item.Icon;
            _itemImage.enabled = true;
            _itemCountText.text = _stackSize > 1 ? _stackSize.ToString() : "";
            _itemNameText.text = item.FishName;
        }
        else
        {
            _itemImage.sprite = null;
            _itemImage.enabled = false;
            _itemCountText.text = "";
            _itemNameText.text = "";
        }

        _itemNameText.gameObject.SetActive(false); // 초기엔 숨김
    }

    #endregion

    #region 마우스 포인터 인터페이스
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (AssignedItem != null)
            _itemNameText.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _itemNameText.gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && AssignedItem != null)
        {
            ReleaseManager.Instance.OpenReleaseConfirm(this); // 현재 슬롯 정보 전달
        }
    }

    #endregion
}
