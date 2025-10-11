using System.Collections.Generic;
using System.Linq;
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
        // OnDisable은 Start에서 구독했다면 그대로 두어도 좋습니다.
        if (EnhancementHolder.Instance != null) // 안전장치 추가
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

    private void Update()
    {
        if (!_game.IsFishing && Input.GetKeyDown(KeyCode.E))
        {
            TogglePanel();
        }
    }

    private void TogglePanel()
    {
        // 현재 패널이 켜져 있는지 확인합니다.
        bool isCurrentlyActive = enhancementPanel.activeSelf;

        // 만약 패널이 켜져 있는 상태에서 끄려고 하는 경우라면,
        if (isCurrentlyActive)
        {
            // UI를 끄기 전에 먼저 모든 재료를 인벤토리로 되돌립니다.
            // 이렇게 하면 OnDisable이 호출되기 전에 모든 데이터 처리와 UI 업데이트 신호가 완료됩니다.
            EnhancementHolder.Instance.EnhancementSystem.ReturnAllMaterialsToInventory();
        }

        // 패널의 활성화 상태를 반전시켜 켜거나 끕니다.
        enhancementPanel.SetActive(!isCurrentlyActive);
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