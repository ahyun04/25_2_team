using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EncyclopediaUI : MonoBehaviour
{
    #region 레퍼런스 연결
    [Header("Data")]
    [SerializeField] private FishDatabaseSO _fishDatabase;

    [Header("UI Groups")]
    [SerializeField] private GameObject _encyclopediaGroup; // 전체 UI (O키로 토글)
    [SerializeField] private GameObject _collectionGroup;
    [SerializeField] private Transform _contentTransform;
    [SerializeField] private RectTransform _scrollViewport;

    [Header("Detail Panel (Learn More)")]
    [SerializeField] private GameObject _learnMoreGroup;    // Learn more Image 그룹

    [Header("Detail Info Elements")]
    [SerializeField] private UIModelViewer _detailModelViewer;
    [SerializeField] private TextMeshProUGUI _nameText;     // Name
    [SerializeField] private TextMeshProUGUI _countText;    // Count (??마리 잡음)
    [SerializeField] private TextMeshProUGUI _hpText;       // HP (체력 : ??)
    [SerializeField] private TextMeshProUGUI _costText;     // 코스트
    [SerializeField] private TextMeshProUGUI _skillNameText;// Skill Name

    [Header("Tab Buttons")]
    [SerializeField] private Button _collectionTabButton;
    [SerializeField] private Button _learnMoreTabButton;

    [Header("Navigation")]
    [SerializeField] private Button _leftButton;
    [SerializeField] private Button _rightButton;
    [SerializeField] private TextMeshProUGUI _moreFishText; // More Fish....

    #endregion
    private List<CollectionSlot_UI> _uiSlots = new List<CollectionSlot_UI>();
    private List<FishSO> _allFishes = new List<FishSO>();
    private List<FishSO> _collectedOnlyList = new List<FishSO>();   
    private List<FishSO> CurrentList => _isViewAllMode ? _allFishes : _collectedOnlyList;
    private int _currentIndex = 0;
    private bool _isViewAllMode = false;

    private void Start()
    {
        // 시작 시 UI 끄기
        if (_encyclopediaGroup != null) _encyclopediaGroup.SetActive(false);
        if (_learnMoreGroup != null) _learnMoreGroup.SetActive(false);
        if (_moreFishText != null) _moreFishText.gameObject.SetActive(false);

        // 버튼 리스너 연결
        _leftButton.onClick.AddListener(OnLeftButtonClicked);
        _rightButton.onClick.AddListener(OnRightButtonClicked);

        _collectionTabButton.onClick.AddListener(OnCollectionTabClicked);
        _learnMoreTabButton.onClick.AddListener(OnLearnMoreTabClicked);

        // 데이터베이스에서 모든 물고기 로드 (ID 순서 혹은 리스트 순서)
        if (_fishDatabase != null)
        {
            _allFishes = _fishDatabase.fishItems;
        }

        InitializeSlots();
    }

    private void InitializeSlots()
    {
        _uiSlots.Clear();

        List<Image> allSlotImages = new List<Image>();
        _contentTransform.GetComponentsInChildren<Image>(true, allSlotImages);

        Image contentImage = _contentTransform.GetComponent<Image>();
        if (contentImage != null && allSlotImages.Contains(contentImage))
        {
            allSlotImages.Remove(contentImage);
        }

        foreach (var img in allSlotImages)
        {
            CollectionSlot_UI slotUI = img.GetComponent<CollectionSlot_UI>();
            if (slotUI == null)
            {
                slotUI = img.gameObject.AddComponent<CollectionSlot_UI>();
            }
            _uiSlots.Add(slotUI);
        }
    }

    private void Update()
    {
        // O키로 도감 켜고 끄기
        if (Input.GetKeyDown(KeyCode.O))
        {
            ToggleEncyclopedia();
        }
    }

    #region 도감 메인 화면 (Grid)

    public void ToggleEncyclopedia()
    {
        bool isActive = !_encyclopediaGroup.activeSelf;
        _encyclopediaGroup.SetActive(isActive);

        if (isActive)
        {
            RefreshGrid();
            OnCollectionTabClicked();
        }
    }

    private void RefreshGrid()
    {
        int fishCount = _allFishes.Count;

        for (int i = 0; i < _uiSlots.Count; i++)
        {
            CollectionSlot_UI slotUI = _uiSlots[i];

            if (i >= fishCount)
            {
                slotUI.gameObject.SetActive(false);
                continue;
            }

            slotUI.gameObject.SetActive(true);
            FishSO fish = _allFishes[i];
            bool isCollected = CollectionManager.Instance.IsFishCollected(fish);

            slotUI.SetupSlot(fish, isCollected, this, _scrollViewport);
        }
    }

    #endregion

    #region 탭 버튼 기능 (Tab Logic)
    private void OnCollectionTabClicked()
    {
        if (_detailModelViewer != null) _detailModelViewer.ClearModel();

        if (_collectionGroup != null) _collectionGroup.SetActive(true);
        if (_learnMoreGroup != null) _learnMoreGroup.SetActive(false);

        RefreshGrid();

        _encyclopediaGroup.SetActive(true);
        _learnMoreGroup.SetActive(false);

        SetButtonChildActive(_collectionTabButton, true);
        SetButtonChildActive(_learnMoreTabButton, false);
    }

    private void OnLearnMoreTabClicked()
    {
        _isViewAllMode = true;

        _currentIndex = 0;

        if (_learnMoreGroup != null) _learnMoreGroup.SetActive(true);
        if (_collectionGroup != null) _collectionGroup.SetActive(false);

        _learnMoreGroup.SetActive(true);
        SetButtonChildActive(_collectionTabButton, false);
        SetButtonChildActive(_learnMoreTabButton, true);

        UpdateDetailUI();
    }

    private void SetButtonChildActive(Button btn, bool isActive)
    {
        if (btn != null && btn.transform.childCount > 0)
        {
            btn.transform.GetChild(0).gameObject.SetActive(isActive);
        }
    }

    #endregion

    #region 상세 정보 패널 (Detail View)
    public void ShowDetailPanel(FishSO fish)
    {
        _isViewAllMode = false;

        UpdateCollectedList();

        if (_collectedOnlyList.Contains(fish))
        {
            _currentIndex = _collectedOnlyList.IndexOf(fish);
        }
        else
        {
            _currentIndex = 0;
        }

        if (_learnMoreGroup != null) _learnMoreGroup.SetActive(true);
        if (_collectionGroup != null) _collectionGroup.SetActive(false);

        SetButtonChildActive(_collectionTabButton, false);
        SetButtonChildActive(_learnMoreTabButton, true);

        UpdateDetailUI();
    }

    private void UpdateCollectedList()
    {
        _collectedOnlyList.Clear();
        foreach (var fish in _allFishes)
        {
            if (CollectionManager.Instance.IsFishCollected(fish))
            {
                _collectedOnlyList.Add(fish);
            }
        }
    }

    private void UpdateDetailUI()
    {
        List<FishSO> targetList = CurrentList;

        if (targetList.Count == 0 || _currentIndex < 0 || _currentIndex >= targetList.Count) return;

        FishSO fish = targetList[_currentIndex];

        bool isCollected = CollectionManager.Instance.IsFishCollected(fish);
        int fishCount = 0;
        if (isCollected && CollectionHolder.Instance != null)
            fishCount = CollectionHolder.Instance.CollectionSystem.GetFishCount(fish);

        if (!isCollected)
        {
            _detailModelViewer.ClearModel();
            _nameText.text = "???";
            _hpText.text = "???";
            _countText.text = "???";
            _costText.text = "???";
            _skillNameText.text = "???";
        }
        else
        {
            _detailModelViewer.ShowCollectionModel(fish.Prefab);
            _nameText.text = fish.Skill_name;
            _hpText.text = $"체력 : {fish.Hp}";
            _countText.text = $"{fishCount}마리 잡음";
            _costText.text = $"행동력 : {fish.AbilityToAct}";
            _skillNameText.text = string.IsNullOrEmpty(fish.Description) ? "스킬 없음" : fish.Description;
        }

        UpdateNavigationButtons();
    }

    private void UpdateNavigationButtons()
    {
        int totalCount = CurrentList.Count;

        if (totalCount <= 1)
        {
            _leftButton.gameObject.SetActive(false);
            _rightButton.gameObject.SetActive(false);
            if (_moreFishText != null) _moreFishText.gameObject.SetActive(true);
            return;
        }

        if (_currentIndex <= 0) _leftButton.gameObject.SetActive(false);
        else _leftButton.gameObject.SetActive(true);

        if (_currentIndex >= totalCount - 1)
        {
            _rightButton.gameObject.SetActive(false);
            if (_moreFishText != null) _moreFishText.gameObject.SetActive(true);
        }
        else
        {
            _rightButton.gameObject.SetActive(true);
            if (_moreFishText != null) _moreFishText.gameObject.SetActive(false);
        }
    }

    private void OnLeftButtonClicked()
    {
        if (_currentIndex > 0)
        {
            _currentIndex--;
            UpdateDetailUI();
        }
    }

    private void OnRightButtonClicked()
    {
        if (_currentIndex < CurrentList.Count - 1)
        {
            _currentIndex++;
            UpdateDetailUI();
        }
    }

    #endregion
}