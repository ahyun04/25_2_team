using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UICooldown : MonoBehaviour
{
    #region 레퍼런스
    [Header("UI 레퍼런스")]
    [SerializeField] private Image _netCooldownImage;
    [SerializeField] private Image _trapCooldownImage;
    [SerializeField] private TextMeshProUGUI _netCooldownText;
    [SerializeField] private Button _netButton;
    [SerializeField] private Button _fishTrapButton;

    [Header("제어할 스크립트")]
    [SerializeField] private NetFishing _netFishing;
    [SerializeField] private FishTrapManager _fishTrapManager;

    #endregion

    #region Awake
    private void Awake()
    {
        if (_netCooldownImage) _netCooldownImage.gameObject.SetActive(false);
        if (_netCooldownText) _netCooldownText.gameObject.SetActive(false);
        if (_trapCooldownImage) _trapCooldownImage.gameObject.SetActive(false);
    }

    #endregion

    #region 구독
    private void OnEnable()
    {
        if (FishingHolder.Instance != null)
        {
            UpdateUI();
        }
    }

    private void OnDisable()
    {
        if (FishingHolder.Instance != null)
        {
            FishingHolder.Instance.FishingSystem.OnFishingStateChanged -= UpdateUI;
        }
    }

    #endregion

    #region 초기화
    void Start()
    {
        if (FishingHolder.Instance != null)
        {
            FishingHolder.Instance.FishingSystem.OnFishingStateChanged += UpdateUI;
        }

        _netButton.onClick.AddListener(() => {
            if (FishingHolder.Instance.FishingSystem.CanUseNet())
            {
                _netFishing.StartNetFishing();
            }
        });

        _fishTrapButton.onClick.AddListener(() => {
            var system = FishingHolder.Instance.FishingSystem;
            if (system.CurrentTrapState == FishingSystem.TrapState.ReadyToCollect)
            {
                _fishTrapManager.CollectAndShowResult();
            }
            else if (system.CanStartTrap())
            {
                system.StartTrap();
            }
        });

        UpdateUI();
    }

    #endregion

    #region 업데이트 UI
    private void UpdateUI()
    {
        if (FishingHolder.Instance == null) return;

        var system = FishingHolder.Instance.FishingSystem;
        _netButton.interactable = system.CanUseNet();

        bool isNetOnCooldown = system.IsNetOnCooldown;
        _netCooldownImage.gameObject.SetActive(isNetOnCooldown);
        _netCooldownText.gameObject.SetActive(isNetOnCooldown);

        if (isNetOnCooldown)
        {
            _netCooldownImage.fillAmount = system.NetCooldownTimer / 15.0f;
            _netCooldownText.text = Mathf.RoundToInt(system.NetCooldownTimer).ToString();
        }
        else
        {
            _netCooldownImage.fillAmount = 0;
        }

        _fishTrapButton.interactable = system.CanStartTrap() || system.CurrentTrapState == FishingSystem.TrapState.ReadyToCollect;
        _trapCooldownImage.gameObject.SetActive(system.IsTrapOnCooldown);
    }

    #endregion
}