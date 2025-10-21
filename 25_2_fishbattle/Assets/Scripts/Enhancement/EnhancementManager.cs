using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnhancementManager : MonoBehaviour
{
    #region 레퍼런스
    [Header("중앙 아이템 (물고기 시계)")]
    [SerializeField] private Image _fishClockImage;
    [SerializeField] private TextMeshProUGUI _fishClockHpText;
    [SerializeField] private FishSO _fishClockItemData;

    [Header("재료 및 버튼")]
    public GameObject enhancementPanel;
    [SerializeField] private List<EnhancementSlot_UI> _materialSlots;
    [SerializeField] private Button _enhanceButton;

    private FishingMiniGame _game;

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
        _game = FindObjectOfType<FishingMiniGame>();
        _enhanceButton.onClick.AddListener(OnEnhanceButtonClick);

        for (int i = 0; i < _materialSlots.Count; i++)
        {
            _materialSlots[i].Initialize(i);
        }

        if (EnhancementHolder.Instance != null)
        {
            EnhancementHolder.Instance.EnhancementSystem.OnEnhancementStateChanged += UpdateUI;
            UpdateUI(); // 첫 UI 상태 업데이트
        }
        else
        {
            Debug.LogError("EnhancementHolder를 찾을 수 없습니다!");
        }
    }

    #endregion

    #region 강화
    // 강화 버튼이 클릭되었을 때 호출될 함수
    private void OnEnhanceButtonClick()
    {
        bool success = EnhancementHolder.Instance.EnhancementSystem.AttemptEnhancement();
        if (success)
        {
            Debug.Log("강화 성공!");
        }
        else
        {
            Debug.LogWarning("재료가 부족합니다.");
        }
    }

    // 데이터가 변경될 때마다 UI를 업데이트하는 함수
    private void UpdateUI()
    {
        var system = EnhancementHolder.Instance.EnhancementSystem;

        // 물고기 시계 HP 업데이트
        _fishClockHpText.text = $"HP: {system.FishClockHp}";
        _fishClockImage.sprite = _fishClockItemData.Icon;
        _fishClockImage.color = Color.white;

        // 재료 슬롯 UI 업데이트
        for (int i = 0; i < _materialSlots.Count; i++)
        {
            var dataSlot = system.MaterialSlots[i];
            var uiSlot = _materialSlots[i];

            if (!dataSlot.IsEmpty)
            {
                uiSlot.UpdateSlot(dataSlot.ItemData);
            }
            else
            {
                uiSlot.ClearSlot();
            }
        }
    }

    #endregion
}