using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReleaseManager : SingletonMono<ReleaseManager>
{
    #region 레퍼런스
    protected override bool DontDestroy => false;

    [Header("패널")]
    [SerializeField] private GameObject _releaseConfirmPanel;      
    [SerializeField] private GameObject _releaseAmountPanel;       

    [Header("입력 필드 및 버튼")]
    [SerializeField] private TextMeshProUGUI _releasedText;
    [SerializeField] private TMP_InputField _amountInputField;
    [SerializeField] private Button _confirmAmountButton;
    [SerializeField] private Button _confirmYesButton;
    [SerializeField] private Button _confirmNoButton;
    [SerializeField] private Button _confirmAllReleaseButton;
    
    private InventorySlot_UI _currentSlot;

    #endregion

    #region 초기화
    protected override void Awake()
    {
        base.Awake();

        // 버튼 이벤트 연결
        _confirmYesButton.onClick.AddListener(OnConfirmYes);
        _confirmNoButton.onClick.AddListener(OnConfirmNo);
        _confirmAmountButton.onClick.AddListener(OnConfirmAmount);
        _confirmAllReleaseButton.onClick.AddListener(OnConfirmAllRelease);
    }

    #endregion

    #region 마우스 이벤트 호출
    // 우클릭 시 호출
    public void OpenReleaseConfirm(InventorySlot_UI slot)
    {
        _currentSlot = slot;

        if (_currentSlot.AssignedItem is FishSO fish)
        {
            _releasedText.text = $"이 {fish.FishName}을(를) 방생하시겠습니까?";
        }

        _releaseConfirmPanel.SetActive(true);
    }

    private void OnConfirmYes()
    {
        _releaseConfirmPanel.SetActive(false);
        _releaseAmountPanel.SetActive(true);
        _amountInputField.text = ""; // 초기화
    }

    private void OnConfirmNo()
    {
        _releaseConfirmPanel.SetActive(false);
    }

    private void OnConfirmAmount()
    {
        if (_currentSlot == null) return;

        if (!int.TryParse(_amountInputField.text, out int releaseCount))
        {
            Debug.LogWarning("숫자를 올바르게 입력해주세요.");
            return;
        }

        if (releaseCount <= 0)
        {
            Debug.LogWarning("방생 수량은 1 이상이어야 합니다.");
            return;
        }

        var inventoryHolder = FindObjectOfType<InventoryHolder>();
        if (inventoryHolder == null)
        {
            Debug.LogError("InventoryHolder를 찾을 수 없습니다.");
            return;
        }

        int currentCount = InventoryManager.Instance.GetFishCount(_currentSlot.AssignedItem);
        if (releaseCount > currentCount)
        {
            Debug.LogWarning("현재 인벤토리에 있는 수량보다 많습니다.");
            return;
        }

        inventoryHolder.InventorySystem.RemoveItem(_currentSlot.AssignedItem, releaseCount);

        _releaseAmountPanel.SetActive(false);

        Debug.Log($"{releaseCount}마리 방생 완료");
    }

    private void OnConfirmAllRelease()
    {
        if (_currentSlot == null) return;

        var inventoryHolder = FindObjectOfType<InventoryHolder>();
        if (inventoryHolder == null)
        {
            Debug.LogError("InventoryHolder를 찾을 수 없습니다.");
            return;
        }

        int currentCount = InventoryManager.Instance.GetFishCount(_currentSlot.AssignedItem);
        if (currentCount <= 0)
        {
            Debug.LogWarning("방생할 수량이 없습니다.");
            return;
        }

        inventoryHolder.InventorySystem.RemoveItem(_currentSlot.AssignedItem, currentCount);

        _releaseConfirmPanel.SetActive(false); // 패널 닫기
        _releaseAmountPanel.SetActive(false);

        Debug.Log($"전체({currentCount}마리) 방생 완료");
    }


    #endregion
}
