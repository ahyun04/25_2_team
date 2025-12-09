using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    #region 레퍼런스
    [SerializeField] private Canvas _mainCanvas;
    [Header("카메라 설정")]
    [SerializeField] private Camera _mainCamera;    
    [SerializeField] private Camera _overlayCamera; 

    [Header("인벤토리 패널")]
    [SerializeField] private GameObject _inventoryPanel;
    [SerializeField] private Button _closePanelButton;
    [SerializeField] private Button _enhancementCloseButton;

    [Header("게임 오버 UI")]
    [SerializeField] private GameObject _gameOverPanel;      // 게임 오버 전체 패널 (배경 포함)
    [SerializeField] private TextMeshProUGUI _endGameText;   // 승리/패배 텍스트
    [SerializeField] private Button _retryButton;            // 재도전 버튼 (패배 시 활성)
    [SerializeField] private Button _toMapButton;            // 맵으로 돌아가기 버튼 (패배/승리 시 활성)
    [SerializeField] private Button _winReturnButton;        // 승리 시 뜨는 버튼   

    [Header("게임 정보 텍스트")]
    [SerializeField] private TextMeshProUGUI _playerAPText;
    [SerializeField] private TextMeshProUGUI _enemyAPText;
    [SerializeField] private TextMeshProUGUI _playerKillCountText;
    [SerializeField] private TextMeshProUGUI _enemyKillCountText;

    [Header("턴 종료 버튼")]
    [SerializeField] private Button _endTurnButton;
    [SerializeField] private GameObject _endTurnLockOverlay;

    [Header("공격 버튼")]
    [SerializeField] private Button _attackButton;
    [SerializeField] private GameObject _attackLockOverlay;

    [Header("힐 버튼")]
    [SerializeField] private Button _healButton;

    #endregion

    #region 초기화
    void Start()
    {
        if (_healButton != null)
        {
            _healButton.gameObject.SetActive(false); // 시작할 땐 숨김
        }

        if (_mainCamera == null) _mainCamera = Camera.main;

        if (_mainCanvas != null && _mainCamera != null)
        {
            _mainCanvas.worldCamera = _mainCamera;
        }

        if (_closePanelButton != null) _closePanelButton.onClick.AddListener(ClosePanle);
        if (_endTurnButton != null) _endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);
        if (_attackButton != null) _attackButton.onClick.AddListener(OnAttackButtonClicked);
        if (_retryButton != null) _retryButton.onClick.AddListener(OnRetryClicked);
        if (_toMapButton != null) _toMapButton.onClick.AddListener(OnToMapClicked);
        if (_winReturnButton != null) _winReturnButton.onClick.AddListener(OnToMapClicked);
        if (_gameOverPanel != null) _gameOverPanel.SetActive(false);

        SetButtonUIsForPlayerTurn(true);
    }

    private void Update()
    {
        if (TurnManager.Instance != null && TurnManager.Instance.IsGameStarted)
        {
            UpdateAPText();
            UpdateKillCountText();
        }
    }

    #endregion

    #region UI 업데이트 메서드
    private void OnEndTurnButtonClicked() => TurnManager.Instance.EndTurn();
    private void OnAttackButtonClicked() => TurnManager.Instance.OnAttackButtonClicked();

    public void SetButtonUIsForPlayerTurn(bool isPlayerTurn)
    {
        // 버튼 자체의 상호작용 가능 여부
        if (_endTurnButton != null)
        {
            _endTurnButton.interactable = isPlayerTurn;
        }
        if (_attackButton != null)
        {
            _attackButton.interactable = isPlayerTurn;
        }

        // 잠금 오버레이 활성화/비활성화
        if (_endTurnLockOverlay != null)
        {
            _endTurnLockOverlay.SetActive(!isPlayerTurn); // 플레이어 턴이 아니면 잠금 활성화
        }
        if (_attackLockOverlay != null)
        {
            _attackLockOverlay.SetActive(!isPlayerTurn); // 플레이어 턴이 아니면 잠금 활성화
        }
    }

    public void UpdateAPText()
    {
        _playerAPText.text = $"AP: {TurnManager.Instance.PlayerAP} / 5";
        _enemyAPText.text = $"AP: {TurnManager.Instance.EnemyAP} / 5";
    }

    public void UpdateKillCountText()
    {
        _playerKillCountText.text = $"{TurnManager.Instance.PlayerKillCount} / 3";
        _enemyKillCountText.text = $"{TurnManager.Instance.EnemyKillCount}/ 3";

        if (TurnManager.Instance.PlayerKillCount >= 3)
        {
            ShowEndGameUI(true);
        }
        else if (TurnManager.Instance.EnemyKillCount >= 3)
        {
            ShowEndGameUI(false);
        }

        TurnManager.Instance.CheckBattlefieldAndEnableBenchDrag();
    }

    private void ShowEndGameUI(bool isPlayerWin)
    {
        if (GameManager.Instance.currentGameState == GameState.GameOver) return;

        GameManager.Instance.GameOver();
        Time.timeScale = 0f;

        if (_mainCanvas != null && _overlayCamera != null)
        {
            _mainCanvas.worldCamera = _overlayCamera;
        }

        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(true);

            if (isPlayerWin)
            {
                // [승리 시]
                _endGameText.text = "<color=yellow>VICTORY!</color>";
                Debug.Log("<color=yellow>[보상] 승리! 보상이 지급됩니다.</color>");

                // 1. 패배용 버튼 2개 끄기
                if (_retryButton != null) _retryButton.gameObject.SetActive(false);
                if (_toMapButton != null) _toMapButton.gameObject.SetActive(false);

                // 2. 승리용 버튼 1개 켜기
                if (_winReturnButton != null) _winReturnButton.gameObject.SetActive(true);
            }
            else
            {
                // [패배 시]
                _endGameText.text = "<color=red>DEFEAT...</color>";

                // 1. 패배용 버튼 2개 켜기
                if (_retryButton != null) _retryButton.gameObject.SetActive(true);
                if (_toMapButton != null) _toMapButton.gameObject.SetActive(true);

                // 2. 승리용 버튼 1개 끄기
                if (_winReturnButton != null) _winReturnButton.gameObject.SetActive(false);
            }
        }
    }

    private void OnRetryClicked()
    {
        Time.timeScale = 1f;

        if (SceneManager.Instance != null)
        {
            SceneManager.Instance.ReloadCurrentScene();
        }
        else
        {
            Debug.LogError("SceneManager 인스턴스를 찾을 수 없습니다!");
        }
    }

    private void OnToMapClicked()
    {
        Time.timeScale = 1f;

        if (SceneManager.Instance != null)
        {
            SceneManager.Instance.LoadMapSelectionSceneName();
        }
        else
        {
            Debug.LogError("SceneManager 인스턴스를 찾을 수 없습니다!");
        }
    }

    public void SetTooltipForCard(GameObject cardObject, bool isActive, bool isFocus)
    {
        if (cardObject.TryGetComponent<CardDisplay>(out var disp))
        {
            disp.SetTooltipActive(isActive, isFocus);
        }
    }

    public void SetAllBattleAndBenchCardTooltips(bool isActive)
    {
        var allPlayerAreas = CardManager.Instance.playerBattleAreas.Cast<CardSlotArea>().Concat(CardManager.Instance.playerBenchAreas.Cast<CardSlotArea>());
        var allEnemyAreas = CardManager.Instance.enemyBattleAreas.Cast<CardSlotArea>().Concat(CardManager.Instance.enemyBenchAreas.Cast<CardSlotArea>());
        var allAreas = allPlayerAreas.Concat(allEnemyAreas);

        foreach (var area in allAreas)
        {
            foreach (var cardObj in area.GetOccupiedCards())
            {
                if (cardObj != null)
                {
                    SetTooltipForCard(cardObj, isActive, false);
                }
            }
        }
    }

    private void ClosePanle()
    {
        _inventoryPanel.SetActive(false);
        ReleaseManager.Instance.CloseReleasePanle();
    }

    #endregion
}
