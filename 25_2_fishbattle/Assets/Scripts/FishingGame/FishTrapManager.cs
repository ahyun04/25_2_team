using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class FishTrapManager : MonoBehaviour
{
    #region 상태값
    public enum TrapState
    {
        Idle,         
        Timing,       
        ReadyToCollect 
    }
    public TrapState CurrentState { get; private set; } = TrapState.Idle;

    #endregion

    #region 레퍼런스
    [Header("UI 요소")]
    [SerializeField] private TextMeshProUGUI _resultText;
    [SerializeField] private TextMeshProUGUI _trapTimerText;    // 좌측 상단 타이머 텍스트
    [SerializeField] private GameObject _resultPanel;           // 결과 창 패널
    [SerializeField] private Transform _contentParent;          // 결과 창의 Content
    [SerializeField] private GameObject _resultFishPrefab;      // 결과 창에 표시될 물고기 프리팹
    [SerializeField] private Button _putInInventoryButton;      // 인벤토리에 넣기 버튼

    [Header("통발 설정")]
    [SerializeField] private float _trapDuration = 30f;
    [SerializeField] private int _catchAmount = 15;

    [Header("레퍼런스")]
    [SerializeField] private FishSpawner _fishSpawner;
    [SerializeField] private InventoryHolder _playerInventory;
    [SerializeField] private NetFishing _netFishing;               
    [SerializeField] private FishingMiniGame _fishingMiniGame; 

    private List<FishSO> _lastCaughtTrapFish;

    #endregion

    #region 초기화
    void Start()
    {
        _trapTimerText.gameObject.SetActive(false);
        _resultPanel.SetActive(false);
    }

    public void StartTrap()
    {
        if (CurrentState != TrapState.Idle) return;
        StartCoroutine(TrapRoutine());
    }

    #endregion

    #region 통발
    // 30초 카운트다운 및 다른 낚시 대기
    private IEnumerator TrapRoutine()
    {
        CurrentState = TrapState.Timing;
        _trapTimerText.gameObject.SetActive(true);
        float timer = _trapDuration;

        while (timer > 0f)
        {
            _trapTimerText.text = $"통발 시간 : {Mathf.CeilToInt(timer)}초";
            timer -= Time.deltaTime;
            yield return null;
        }

        _trapTimerText.gameObject.SetActive(false);
        Debug.Log("통발 시간 끝! 다른 낚시가 끝날 때까지 대기합니다...");

        // 일반 낚시나 그물 낚시가 끝날 때까지 여기서 계속 대기합니다.
        while (_netFishing.IsMiniGameRunning || (_fishingMiniGame != null && _fishingMiniGame.IsMiniGameRunning))
        {
            yield return null; // 다음 프레임까지 기다림
        }

        if (_fishingMiniGame != null)
        {
            while (_fishingMiniGame.IsResultPanelActive)
            {
                yield return null; // 결과 창이 활성화되어 있는 동안 계속 대기
            }
        }

        Debug.Log("모든 낚시가 종료되었습니다. 통발 결과를 표시합니다.");
        yield return new WaitForSeconds(0.7f);
        StartCoroutine(ShowTrapResultRoutine());
    }

    // 결과 창 표시 및 물고기 목록 생성
    private IEnumerator ShowTrapResultRoutine()
    {
        _lastCaughtTrapFish = new List<FishSO>();

        // 물고기 15마리를 리스트에 임시 저장
        for (int i = 0; i < _catchAmount; i++)
        {
            FishSO caughtFish = _fishSpawner.GetRandomFishByScene();
            if (caughtFish != null)
            {
                _lastCaughtTrapFish.Add(caughtFish);
            }
        }

        // 이전에 있던 결과물 삭제
        foreach (Transform child in _contentParent)
        {
            Destroy(child.gameObject);
        }

        // 리스트의 물고기를 UI로 생성
        foreach (FishSO fish in _lastCaughtTrapFish)
        {
            GameObject fishUIObject = Instantiate(_resultFishPrefab, _contentParent);
            fishUIObject.GetComponent<ResultFishUI>().SetData(fish);
        }

        // 결과 창을 켜고, 버튼 리스너 설정
        _resultPanel.SetActive(true);
        _resultText.text = $"통발 결과창";
        _putInInventoryButton.onClick.RemoveAllListeners();
        _putInInventoryButton.onClick.AddListener(AddTrapFishToInventory);

        yield return null;
    }

    public void AddTrapFishToInventory()
    {
        if (_lastCaughtTrapFish == null) return;

        foreach (FishSO fish in _lastCaughtTrapFish)
        {
            _playerInventory.InventorySystem.AddToInventory(fish, 1);
        }
        Debug.Log("통발로 잡은 물고기를 인벤토리에 넣었습니다.");

        _resultPanel.SetActive(false);
        _resultText.text = $"";
        _lastCaughtTrapFish.Clear();
        CurrentState = TrapState.Idle; // 모든 과정이 끝나면 상태를 초기화
    }

    #endregion
}