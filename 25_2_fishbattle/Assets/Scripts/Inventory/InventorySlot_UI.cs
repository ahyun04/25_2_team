using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot_UI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    #region 레퍼런스
    [SerializeField] private Image _itemImage;
    [SerializeField] private TextMeshProUGUI _itemCountText;
    [SerializeField] private TextMeshProUGUI _itemNameText;

    public FishSO AssignedItem { get; private set; }
    public int StackSize { get; private set; }

    [Header("드래그 시 시각적 피드백을 위한 변수")]
    private static GameObject _draggedSlotClone;
    private Canvas _rootCanvas;
    private CanvasGroup _canvasGroup; // 원본 슬롯의 투명도를 조절하기 위함

    #endregion

    #region 업데이트 슬롯
    private void Awake()
    {
        // 최상위 Canvas를 찾아 저장해둡니다. 드래그 아이콘을 그 위에 그려야 하기 때문입니다.
        _rootCanvas = GetComponentInParent<Canvas>();

        // CanvasGroup이 없으면 추가하고, 있으면 가져옵니다.
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void UpdateSlot(FishSO item, int _stackSize)
    {
        AssignedItem = item;
        StackSize = _stackSize;

        if (item != null)
        {
            _itemImage.sprite = item.Icon;
            _itemImage.enabled = true;
            _itemImage.raycastTarget = true; // 드래그를 위해 Raycast Target을 켜야 합니다.
            _itemCountText.text = _stackSize > 1 ? _stackSize.ToString() : "";
            _itemNameText.text = item.Skill_name;
        }
        else
        {
            _itemImage.sprite = null;
            _itemImage.enabled = false;
            _itemImage.raycastTarget = false; // 빈 슬롯은 반응하지 않도록 합니다.
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
        else if (eventData.button == PointerEventData.InputButton.Left && AssignedItem != null)
        {
            if (TradeManager.Instance != null)
            {
                TradeManager.Instance.OnPlayerItemSelected(this);
            }
        }
    }

    #endregion

    #region 드래그 앤 드롭 인터페이스
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (AssignedItem == null) return;

        // 복제본 생성
        _draggedSlotClone = Instantiate(gameObject, _rootCanvas.transform);
        _draggedSlotClone.name = "Dragged Slot Clone";
        _draggedSlotClone.GetComponent<RectTransform>().sizeDelta = GetComponent<RectTransform>().sizeDelta;
        _draggedSlotClone.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

        var cloneSlotScript = _draggedSlotClone.GetComponent<InventorySlot_UI>();
        if (cloneSlotScript != null)
        {
            // 개수 텍스트는 보이지 않게 비활성화합니다.
            if (cloneSlotScript._itemCountText != null)
                cloneSlotScript._itemCountText.gameObject.SetActive(false);

            // 이름 텍스트는 항상 보이도록 활성화합니다.
            if (cloneSlotScript._itemNameText != null)
                cloneSlotScript._itemNameText.gameObject.SetActive(true);
        }

        // 복제본을 Hierarchy의 맨 마지막으로 보내 가장 위에 보이게 함
        _draggedSlotClone.transform.SetAsLastSibling();

        // 복제본 반투명 처리 및 레이캐스트 비활성화
        var cloneCanvasGroup = _draggedSlotClone.GetComponent<CanvasGroup>();
        cloneCanvasGroup.alpha = 0.7f;
        cloneCanvasGroup.blocksRaycasts = false;

        // 원본 슬롯 숨기기
        _canvasGroup.alpha = 0.3f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_draggedSlotClone != null)
        {
            var rectTransform = _draggedSlotClone.GetComponent<RectTransform>();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rootCanvas.transform as RectTransform,
                eventData.position,
                _rootCanvas.worldCamera,
                out Vector2 localPos
            );

            rectTransform.localPosition = localPos;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 원본을 다시 선명하게 복구
        _canvasGroup.alpha = 1f;

        // 마우스를 따라다니던 복제본 제거
        if (_draggedSlotClone != null)
        {
            Destroy(_draggedSlotClone);
        }
        _draggedSlotClone = null;
    }

    #endregion
}
