using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnhancementManager : MonoBehaviour
{
    #region 레퍼런스
    [Header("강화 재료 슬롯")]
    [SerializeField] private List<EnhancementSlot_UI> _materialSlots;

    [Header("중앙 결과 슬롯")]
    [SerializeField] private EnhancementResultSlot_UI _resultSlotUI;

    [Header("조작 버튼 및 텍스트")]
    [SerializeField] private Button _enhanceButton;  
    [SerializeField] private TextMeshProUGUI _warningText; 

    private FishSO _pendingResultItem;

    #endregion

    #region 초기화
    private void OnDisable()
    {
        if (EnhancementHolder.Instance != null)
        {
            EnhancementHolder.Instance.EnhancementSystem.OnEnhancementStateChanged -= UpdateUI;
        }
    }

    private void Start()
    {
        _enhanceButton.onClick.AddListener(OnEnhanceButtonClick);
        _resultSlotUI.Initialize(this);

        // 재료 슬롯 초기화
        for (int i = 0; i < _materialSlots.Count; i++)
        {
            _materialSlots[i].Initialize(i);
        }

        _warningText.text = "";

        if (EnhancementHolder.Instance != null)
        {
            EnhancementHolder.Instance.EnhancementSystem.OnEnhancementStateChanged += UpdateUI;
            UpdateUI();
        }
        else
        {
            Debug.LogError("EnhancementHolder를 찾을 수 없습니다!");
        }
    }

    #endregion

    #region 로직: 강화 버튼 클릭
    private void OnEnhanceButtonClick()
    {
        if (_pendingResultItem != null)
        {
            ShowWarning("먼저 완성된 아이템을 수령해주세요!");
            return;
        }

        var system = EnhancementHolder.Instance.EnhancementSystem;

        string errorMsg = system.ValidateEnhancement();
        if (!string.IsNullOrEmpty(errorMsg))
        {
            ShowWarning(errorMsg);
            return;
        }

        FishSO result = system.AttemptEnhancement();
        if (result != null)
        {
            SetResultSlot(result);
            Debug.Log("강화 성공! 결과 이미지를 클릭해서 수령하세요.");
        }
    }
    #endregion

    #region 로직: 결과 아이템 수령 (UI에서 호출됨)
    public void OnResultSlotClick()
    {
        if (_pendingResultItem == null) return;

        if (InventoryHolder.Instance == null)
        {
            ShowWarning("인벤토리 시스템을 찾을 수 없습니다.");
            return;
        }

        bool isSuccess = InventoryHolder.Instance.InventorySystem.AddToInventory(_pendingResultItem, 1);

        if (isSuccess)
        {
            string earnedItemName = _pendingResultItem.Skill_name;
            ClearResultSlot();
            ShowWarning($"{earnedItemName}을(를) 획득했습니다!");
        }
        else
        {
            ShowWarning("인벤토리가 가득 찼습니다!");
        }
    }
    #endregion

    #region UI 유틸리티
    private void SetResultSlot(FishSO item)
    {
        _pendingResultItem = item;
        _resultSlotUI.SetItem(item);
    }

    private void ClearResultSlot()
    {
        _pendingResultItem = null;
        _resultSlotUI.Clear();
    }

    private void ShowWarning(string message)
    {
        StopAllCoroutines();
        StartCoroutine(WarningCoroutine(message));
    }

    private IEnumerator WarningCoroutine(string message)
    {
        _warningText.text = message;
        _warningText.color = Color.red;
        yield return new WaitForSeconds(2f);
        _warningText.text = "";
    }

    private void UpdateUI()
    {
        var system = EnhancementHolder.Instance.EnhancementSystem;

        for (int i = 0; i < _materialSlots.Count; i++)
        {
            var dataSlot = system.MaterialSlots[i];
            if (!dataSlot.IsEmpty)
                _materialSlots[i].UpdateSlot(dataSlot.ItemData);
            else
                _materialSlots[i].ClearSlot();
        }
    }

    #endregion
}