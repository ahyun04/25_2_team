using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UICooldown : MonoBehaviour
{
    #region 레퍼런스
    [Header("UI 레퍼런스")]
    [SerializeField] private Image _netCooldownImage;
    [SerializeField] private Image _trapCooldownImage;
    [SerializeField] private Image _generalCooldownImage; 
    [SerializeField] private TextMeshProUGUI _netCooldownText;
    [SerializeField] private Button _netButton;
    [SerializeField] private Button _fishTrapButton;
    [SerializeField] private Button _generalFishingButton; 

    [Header("제어할 스크립트")]
    [SerializeField] private NetFishing _netFishing;
    [SerializeField] private FishTrapManager _fishTrapManager;
    [SerializeField] private FishingMiniGame _fishingMiniGame; 

    #endregion

    #region Awake
    private void Awake()
    {
        if (_netCooldownImage) _netCooldownImage.gameObject.SetActive(false);
        if (_netCooldownText) _netCooldownText.gameObject.SetActive(false);
        if (_trapCooldownImage) _trapCooldownImage.gameObject.SetActive(false);
        if (_generalCooldownImage) _generalCooldownImage.gameObject.SetActive(false); 

        _netButton.onClick.AddListener(OnNetButtonClick);
        _fishTrapButton.onClick.AddListener(OnTrapButtonClick);
        _generalFishingButton.onClick.AddListener(OnGeneralFishingClick);
    }
    #endregion

    #region 구독
    private void OnEnable()
    {
        if (FishingHolder.Instance != null)
        {
            FishingHolder.Instance.FishingSystem.OnFishingStateChanged += UpdateUI;
        }

        UpdateUI();
    }

    private void OnDisable()
    {
        if (FishingHolder.Instance != null)
        {
            FishingHolder.Instance.FishingSystem.OnFishingStateChanged -= UpdateUI;
        }
    }
    #endregion

    #region 버튼 클릭 핸들러
    private void OnNetButtonClick()
    {
        if (FishingHolder.Instance.FishingSystem.CanUseNet())
        {
            _netFishing.StartNetFishing();
        }
    }

    private void OnTrapButtonClick()
    {
        var system = FishingHolder.Instance.FishingSystem;

        // 수확 가능 상태(ReadyToCollect)일 때
        if (system.CurrentTrapState == FishingSystem.TrapState.ReadyToCollect)
        {
            // 다른 미니게임이 실행 중인지 확인
            if (MiniGameManager.IsMiniGameRunning)
            {
                MiniGameManager.TryStartMiniGame();
                return;
            }

            // 미니게임이 실행 중이 아니면 수확
            _fishTrapManager.CollectAndShowResult();
        }
        // 통발 설치 가능 상태일 때
        else if (system.CanStartTrap())
        {
            // 다른 미니게임이 실행 중인지 확인
            if (MiniGameManager.IsMiniGameRunning)
            {
                MiniGameManager.TryStartMiniGame();
                return;
            }

            // 미니게임이 실행 중이 아니면 설치
            system.StartTrap();
        }
    }

    private void OnGeneralFishingClick()
    {
        _fishingMiniGame.StartFishing();
    }

    #endregion

    #region 업데이트 UI
    private void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (FishingHolder.Instance == null) return;

        bool isDeepSea = DeepSeaManager.Instance != null;

        var system = FishingHolder.Instance.FishingSystem;

        bool isNetActive = _netFishing != null && _netFishing.IsMiniGameRunning;

        // --- 그물 낚시 ---
        bool isNetOnCooldown = system.IsNetOnCooldown;
        _netButton.interactable = system.CanUseNet() && !isDeepSea;

        _netCooldownImage.gameObject.SetActive(isNetActive || isNetOnCooldown);
        _netCooldownText.gameObject.SetActive(isNetOnCooldown);

        if (isNetOnCooldown)
        {
            _netCooldownImage.fillAmount = system.NetCooldownTimer / 15.0f;
            _netCooldownText.text = Mathf.RoundToInt(system.NetCooldownTimer).ToString();
        }
        else if (isNetActive)
        {
            _netCooldownImage.fillAmount = 1;
        }
        else
        {
            _netCooldownImage.fillAmount = 0;
        }

        // --- 통발 ---
        bool canUseTrap = system.CanStartTrap() || system.CurrentTrapState == FishingSystem.TrapState.ReadyToCollect;
        _fishTrapButton.interactable = canUseTrap && !isDeepSea;
        _trapCooldownImage.gameObject.SetActive(system.IsTrapOnCooldown);

        // --- 일반 낚시 ---
        if (_fishingMiniGame != null)
        {
            _generalFishingButton.interactable = true;

            bool isGeneralActive = _fishingMiniGame.IsMiniGameRunning;

            if (_generalCooldownImage)
            {
                _generalCooldownImage.gameObject.SetActive(isGeneralActive);
                _generalCooldownImage.fillAmount = isGeneralActive ? 1 : 0;
            }
        }
    }

    #endregion
}   