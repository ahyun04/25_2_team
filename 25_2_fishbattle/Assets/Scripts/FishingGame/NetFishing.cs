using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class NetFishing : MonoBehaviour
{
    #region 레퍼런스
    [Header("UI 요소")]
    [SerializeField] private TextMeshProUGUI _statusText;       // 현재 상태 알림 텍스트
    [SerializeField] private GameObject _netMinigamePanel;      // 미니게임 UI 패널
    [SerializeField] private TextMeshProUGUI _mashCountText;    // 연타 횟수 표시 텍스트
    [SerializeField] private TextMeshProUGUI _timerText;        // 남은 시간 표시 텍스트

    [Header("미니게임 설정")]
    [SerializeField] private float _minWaitTime = 5f;           // 최소 대기 시간
    [SerializeField] private float _maxWaitTime = 15f;          // 최대 대기 시간
    [SerializeField] private float _reactionTimeLimit = 2f;     // 반응해야 하는 시간
    [SerializeField] private float _mashTimeLimit = 10f;        // 연타 제한 시간
    [SerializeField] private int _mashTarget = 100;             // 목표 연타 횟수
    [SerializeField] private int _catchAmount = 10;             // 잡을 물고기 수

    [Header("결과 창 UI")]
    [SerializeField] private TextMeshProUGUI _resultText;
    [SerializeField] private GameObject _resultPanel;
    [SerializeField] private Transform _contentParent; 
    [SerializeField] private GameObject _resultFishPrefab; 
    [SerializeField] private Button _putInInventoryButton;

    [Header("레퍼런스")]
    [SerializeField] private FishSpawner _fishSpawner;        
    [SerializeField] private InventoryHolder _playerInventory; 
    [SerializeField] private UICooldown _uiCooldown;

    private Coroutine _netFishingCoroutine;
    public bool IsMiniGameRunning => _netFishingCoroutine != null;
    private int _currentMashCount = 0;

    private List<FishSO> _lastCaughtNetFish;

    #endregion

    #region 초기화
    void Start()
    {
        _playerInventory = InventoryHolder.Instance;

        if (_fishSpawner == null)
        {
            _fishSpawner = FindObjectOfType<FishSpawner>();
        }

        _statusText.gameObject.SetActive(false);
        _netMinigamePanel.SetActive(false);
    }

    #endregion

    #region 그물 낚시
    // 그물 낚시 시작
    public void StartNetFishing()
    {
        if (!MiniGameManager.TryStartMiniGame()) return;

        if (_netFishingCoroutine == null)
        {
            _netFishingCoroutine = StartCoroutine(FishingWithNetRoutine());
        }
    }

    // 물고기 떼를 기다리는 전체 과정
    private IEnumerator FishingWithNetRoutine()
    {
        _statusText.gameObject.SetActive(true);
        Debug.Log("그물을 던졌다... 물고기 떼를 기다리는 중...");

        // 랜덤 시간 대기
        float waitTime = Random.Range(_minWaitTime, _maxWaitTime);
        yield return new WaitForSeconds(waitTime);

        _statusText.text = "물고기 떼가 그물에 걸렸다! 지금이야!";

        yield return StartCoroutine(WaitForInitialInput());
    }

    // 제한 시간 내 스페이스바 입력을 기다리는 함수
    private IEnumerator WaitForInitialInput()
    {
        float timer = _reactionTimeLimit;
        bool inputReceived = false;

        while (timer > 0f)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                inputReceived = true;
                break;
            }
            timer -= Time.deltaTime;
            yield return null;
        }

        _statusText.gameObject.SetActive(false);

        if (inputReceived)
        {
            // 반응 성공 시, 연타 미니게임 시작
            FishingHolder.Instance.FishingSystem.StartNetCooldown();
            Debug.Log("반응 성공! 미니게임 시작!");
            _netMinigamePanel.SetActive(true);
            StartCoroutine(NetMinigameRoutine());
        }
        else
        {
            // 반응 실패
            Debug.Log("놓쳤다... 물고기들이 모두 도망갔다.");
            ResetFishing();
        }
    }

    // 스페이스바 연타 미니게임
    private IEnumerator NetMinigameRoutine()
    {
        _currentMashCount = 0;
        float timer = _mashTimeLimit;

        while (timer > 0f)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _currentMashCount++;

                // 목표 달성 시 즉시 성공 처리
                if (_currentMashCount >= _mashTarget)
                {
                    _netMinigamePanel.SetActive(false); 
                    _statusText.gameObject.SetActive(true);
                    _statusText.text = "성공!";                  

                    yield return new WaitForSeconds(1f);        

                    _statusText.gameObject.SetActive(false);

                    StartCoroutine(CatchFishWithNet());
                    yield break;
                }
            }

            _mashCountText.text = $"연타: {_currentMashCount} / {_mashTarget}";
            _timerText.text = $"남은 시간: {timer:F2}초";

            timer -= Time.deltaTime;
            yield return null;
        }

        _netMinigamePanel.SetActive(false);
        Debug.Log("그물질 실패... 힘이 부족했다.");
        ResetFishing();
    }

    // 물고기 획득 및 인벤토리 추가
    private IEnumerator CatchFishWithNet()
    {
        _lastCaughtNetFish = new List<FishSO>();
        // 물고기 10마리를 잡아 리스트에 임시 저장
        for (int i = 0; i < _catchAmount; i++)
        {
            FishSO caughtFish = _fishSpawner.GetRandomFishByScene();
            if (caughtFish != null)
            {
                _lastCaughtNetFish.Add(caughtFish);
            }
        }

        // 결과 창 Content에 이전에 있던 아이템들 삭제
        foreach (Transform child in _contentParent)
        {
            Destroy(child.gameObject);
        }

        // 리스트에 있는 물고기들을 UI에 표시
        foreach (FishSO fish in _lastCaughtNetFish)
        {
            GameObject fishUIObject = Instantiate(_resultFishPrefab, _contentParent);
            fishUIObject.GetComponent<ResultFishUI>().SetData(fish);
        }

        // 결과 창을 켜고, 버튼 리스너 설정
        _resultPanel.SetActive(true);
        _resultText.text = $"그물 결과창";
        _putInInventoryButton.onClick.RemoveAllListeners();
        _putInInventoryButton.onClick.AddListener(AddNetFishToInventory);

        yield return null;
    }

    // 낚시 상태 초기화
    private void ResetFishing()
    {
        _statusText.text = "";
        _netFishingCoroutine = null;

        MiniGameManager.EndMiniGame();
    }

    public void AddNetFishToInventory()
    {
        if (_lastCaughtNetFish == null) return;

        foreach (FishSO fish in _lastCaughtNetFish)
        {
            _playerInventory.InventorySystem.AddToInventory(fish, 1);
        }
        Debug.Log("그물로 잡은 물고기를 인벤토리에 넣었습니다.");

        _resultPanel.SetActive(false); // 결과 창 닫기
        _resultText.text = "";
        _lastCaughtNetFish.Clear(); // 임시 리스트 비우기
        ResetFishing(); // 낚시 상태 초기화
    }

    // 강제 종료 시 결과 창도 닫도록 수정
    public void ForceStopMinigame()
    {
        if (_netFishingCoroutine == null) return;
        Debug.Log("통발로 인해 그물 낚시가 중단되었습니다.");
        StopCoroutine(_netFishingCoroutine);

        _resultPanel.SetActive(false); // 결과 창 닫기 추가
        _statusText.gameObject.SetActive(false);
        _netMinigamePanel.SetActive(false);
        ResetFishing();
    }

    #endregion
}