// FishTrapManager.cs (완전히 교체)
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class FishTrapManager : MonoBehaviour
{
    #region 레퍼런스
    [Header("UI 요소")]
    [SerializeField] private TextMeshProUGUI _resultText;
    [SerializeField] private TextMeshProUGUI _trapTimerText;
    [SerializeField] private GameObject _resultPanel;
    [SerializeField] private Transform _contentParent;
    [SerializeField] private GameObject _resultFishPrefab;
    [SerializeField] private Button _putInInventoryButton;

    [Header("통발 설정")]
    [SerializeField] private int _catchAmount = 15;

    [Header("다른 낚시 스크립트 레퍼런스")]
    [SerializeField] private FishSpawner _fishSpawner;
    [SerializeField] private NetFishing _netFishing;
    [SerializeField] private FishingMiniGame _fishingMiniGame;

    private List<FishSO> _lastCaughtTrapFish;
    private bool _isWaitingToShowResult = false; // 중복 실행 방지 플래그
    #endregion

    #region 초기화 및 업데이트
    void Start()
    {
        _trapTimerText.gameObject.SetActive(false);
        _resultPanel.SetActive(false);
    }

    void Update()
    {
        var system = FishingHolder.Instance.FishingSystem;

        // 통발 타이머 UI 업데이트
        if (system.CurrentTrapState == FishingSystem.TrapState.Timing)
        {
            _trapTimerText.gameObject.SetActive(true);
            _trapTimerText.text = $"통발 시간 : {Mathf.CeilToInt(system.TrapTimer)}초";
        }
        else
        {
            _trapTimerText.gameObject.SetActive(false);
        }

        // 통발이 수확 가능 상태가 되면, 다른 낚시가 끝날 때까지 기다렸다가 결과창을 보여주는 코루틴을 시작
        if (system.CurrentTrapState == FishingSystem.TrapState.ReadyToCollect && !_isWaitingToShowResult)
        {
            StartCoroutine(WaitForOtherFishingAndShowResult());
        }
    }
    #endregion

    #region 통발 결과 표시
    public void CollectAndShowResult()
    {
        FishingHolder.Instance.FishingSystem.CollectTrap();
    }

    private IEnumerator WaitForOtherFishingAndShowResult()
    {
        _isWaitingToShowResult = true;
        Debug.Log("통발 시간 끝! 다른 낚시가 끝날 때까지 대기합니다...");

        // 다른 낚시 미니게임이 모두 끝날 때까지 대기
        yield return new WaitUntil(() =>
            (_netFishing == null || !_netFishing.IsMiniGameRunning) &&
            (_fishingMiniGame == null || !_fishingMiniGame.IsMiniGameRunning));

        // 일반 낚시 결과창이 닫힐 때까지 대기
        if (_fishingMiniGame != null)
        {
            yield return new WaitUntil(() => !_fishingMiniGame.IsResultPanelActive);
        }

        Debug.Log("모든 낚시가 종료되었습니다. 통발 결과를 표시합니다.");
        yield return new WaitForSeconds(0.7f);

        // 데이터 시스템에 수확했다고 알림
        FishingHolder.Instance.FishingSystem.CollectTrap();
        // 결과창 표시 코루틴 시작
        StartCoroutine(ShowTrapResultRoutine());
    }

    private IEnumerator ShowTrapResultRoutine()
    {
        _lastCaughtTrapFish = new List<FishSO>();
        for (int i = 0; i < _catchAmount; i++)
        {
            FishSO caughtFish = _fishSpawner.GetRandomFishByScene();
            if (caughtFish != null) _lastCaughtTrapFish.Add(caughtFish);
        }

        foreach (Transform child in _contentParent) Destroy(child.gameObject);

        foreach (FishSO fish in _lastCaughtTrapFish)
        {
            GameObject fishUIObject = Instantiate(_resultFishPrefab, _contentParent);
            fishUIObject.GetComponent<ResultFishUI>().SetData(fish);
        }

        _resultPanel.SetActive(true);
        _resultText.text = "통발 결과창";
        _putInInventoryButton.onClick.RemoveAllListeners();
        _putInInventoryButton.onClick.AddListener(AddTrapFishToInventory);
        yield return null;
    }

    public void AddTrapFishToInventory()
    {
        if (_lastCaughtTrapFish == null) return;

        var inventorySystem = InventoryHolder.Instance.InventorySystem;
        foreach (FishSO fish in _lastCaughtTrapFish)
        {
            inventorySystem.AddToInventory(fish, 1);
        }
        Debug.Log("통발로 잡은 물고기를 인벤토리에 넣었습니다.");

        _resultPanel.SetActive(false);
        _resultText.text = "";
        _lastCaughtTrapFish.Clear();
        _isWaitingToShowResult = false; // 플래그 초기화
    }
    #endregion
}