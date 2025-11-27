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
    [SerializeField] private Transform _contentTransform;   // 슬롯이 생성될 부모 (Grid Layout)

    [Header("Detail Panel (Learn More)")]
    [SerializeField] private GameObject _learnMoreGroup;    // Learn more Image 그룹

    [Header("Detail Info Elements")]
    [SerializeField] private Image _detailFishImage;        // FishImage
    [SerializeField] private TextMeshProUGUI _nameText;     // Name
    [SerializeField] private TextMeshProUGUI _countText;    // Count (??마리 잡음)
    [SerializeField] private TextMeshProUGUI _hpText;       // HP (체력 : ??)
    [SerializeField] private TextMeshProUGUI _descText;     // Description
    [SerializeField] private TextMeshProUGUI _skillNameText;// Skill Name

    [Header("Tab Buttons")]
    [SerializeField] private Button _collectionTabButton;
    [SerializeField] private Button _learnMoreTabButton;

    [Header("Navigation")]
    [SerializeField] private Button _leftButton;
    [SerializeField] private Button _rightButton;
    [SerializeField] private TextMeshProUGUI _moreFishText; // More Fish....

    #endregion

    private List<FishSO> _allFishes = new List<FishSO>();
    private int _currentIndex = 0;

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
        List<Image> allSlotImages = new List<Image>();
        _contentTransform.GetComponentsInChildren<Image>(true, allSlotImages);

        Image contentImage = _contentTransform.GetComponent<Image>();
        if (contentImage != null && allSlotImages.Contains(contentImage))
        {
            allSlotImages.Remove(contentImage);
        }

        int fishCount = _allFishes.Count;

        for (int i = 0; i < allSlotImages.Count; i++)
        {
            GameObject slotObj = allSlotImages[i].gameObject;

            // 데이터가 더 이상 없으면 해당 슬롯(이미지) 끄기
            if (i >= fishCount)
            {
                slotObj.SetActive(false);
                continue;
            }

            slotObj.SetActive(true);
            FishSO fish = _allFishes[i];

            CollectionSlot_UI slotUI = slotObj.GetComponent<CollectionSlot_UI>();
            if (slotUI == null)
            {
                slotUI = slotObj.AddComponent<CollectionSlot_UI>();
            }

            bool isCollected = CollectionManager.Instance.IsFishCollected(fish);

            slotUI.SetupSlot(fish, isCollected, this);
        }
    }

    #endregion

    #region 탭 버튼 기능 (Tab Logic)

    // Collection 버튼 눌렀을 때
    private void OnCollectionTabClicked()
    {
        // 1. 그리드 새로고침 및 표시
        RefreshGrid();

        // 2. 패널 상태 설정 (목록 켜기, 상세 끄기)
        // _encyclopediaGroup은 이미 켜져있다고 가정하지만 안전하게 한 번 더 켭니다.
        _encyclopediaGroup.SetActive(true);
        _learnMoreGroup.SetActive(false);

        // 3. 버튼 자식 이미지(하이라이트) 상태 변경
        SetButtonChildActive(_collectionTabButton, true);
        SetButtonChildActive(_learnMoreTabButton, false);
    }

    // Learn More Fish 버튼 눌렀을 때
    private void OnLearnMoreTabClicked()
    {
        // 1. 상세 정보 표시
        ShowDetailPanel(_allFishes[_currentIndex]);

        // 2. 버튼 자식 이미지 상태 변경
        SetButtonChildActive(_collectionTabButton, false);
        SetButtonChildActive(_learnMoreTabButton, true);
    }

    // 버튼의 첫 번째 자식(Image)을 켜고 끄는 헬퍼 함수
    private void SetButtonChildActive(Button btn, bool isActive)
    {
        if (btn != null && btn.transform.childCount > 0)
        {
            btn.transform.GetChild(0).gameObject.SetActive(isActive);
        }
    }

    #endregion

    #region 상세 정보 패널 (Detail View)

    // 슬롯을 클릭했을 때 호출됨
    public void ShowDetailPanel(FishSO fish)
    {
        // 해당 물고기의 인덱스 찾기
        _currentIndex = _allFishes.IndexOf(fish);

        // 상세 패널 및 관련 오브젝트 켜기
        _learnMoreGroup.SetActive(true);

        SetButtonChildActive(_collectionTabButton, false);
        SetButtonChildActive(_learnMoreTabButton, true);

        UpdateDetailUI();
    }

    private void UpdateDetailUI()
    {
        // 인덱스 안전 장치
        if (_currentIndex < 0 || _currentIndex >= _allFishes.Count) return;

        FishSO fish = _allFishes[_currentIndex];

        // 수집 여부 데이터 가져오기
        bool isCollected = CollectionManager.Instance.IsFishCollected(fish);
        int count = CollectionManager.Instance.IsFishCollected(fish)
                    ? CollectionHolder.Instance.CollectionSystem.GetFishCount(fish)
                    : 0;

        // UI 텍스트 갱신
        // 수집하지 못한 물고기로 네비게이션 해왔을 경우 정보 숨김 (선택사항)
        if (!isCollected)
        {
            _nameText.text = "???";
            _hpText.text = "체력 : ???";
            _countText.text = "0마리 잡음";
            _descText.text = "아직 발견하지 못한 물고기입니다.";
            _skillNameText.text = "스킬 : ???";
        }
        else
        {
            _detailFishImage.sprite = fish.Icon;
            _nameText.text = fish.Name;
            _hpText.text = $"체력 : {fish.Hp}";
            _countText.text = $"{count}마리 잡음";
            _descText.text = fish.Description;
            _skillNameText.text = string.IsNullOrEmpty(fish.Skill_name) ? "스킬 없음" : fish.Skill_name;
        }

        UpdateNavigationButtons();
    }

    private void UpdateNavigationButtons()
    {
        int collectedCount = CollectionManager.Instance.GetCollectedCount();
        int totalDatabaseCount = _allFishes.Count;

        if (collectedCount <= 1)
        {
            _leftButton.gameObject.SetActive(false);
            _rightButton.gameObject.SetActive(false);

            if (_moreFishText != null) _moreFishText.gameObject.SetActive(true);

            return;
        }

        if (_currentIndex <= 0)
        {
            _leftButton.gameObject.SetActive(false);
        }
        else
        {
            _leftButton.gameObject.SetActive(true);
        }

        if (_currentIndex >= totalDatabaseCount - 1)
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
        if (_currentIndex < _allFishes.Count - 1)
        {
            _currentIndex++;
            UpdateDetailUI();
        }
    }

    #endregion
}