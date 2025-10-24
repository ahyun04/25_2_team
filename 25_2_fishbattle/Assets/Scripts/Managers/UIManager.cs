using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    #region 레퍼런스
    [Header("인벤토리 패널")]
    [SerializeField] private GameObject _inventoryPanel;
    [SerializeField] private Button _closePanelButton;
    [SerializeField] private Button _enhancementCloseButton;

    [Header("엔드 게임 텍스트")]
    public TextMeshProUGUI endGameText;

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

    #endregion

    #region 초기화
    void Start()
    {
        if (_closePanelButton != null)
            _closePanelButton.onClick.AddListener(ClosePanle);

        if (_endTurnButton != null) 
            _endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);

        if (_attackButton != null)
            _attackButton.onClick.AddListener(OnAttackButtonClicked);

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
    private void OnEndTurnButtonClicked()
    {
        TurnManager.Instance.EndTurn();
    }

    private void OnAttackButtonClicked()
    {
        TurnManager.Instance.OnAttackButtonClicked();
    }

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

        endGameText.gameObject.SetActive(true);
        endGameText.text = isPlayerWin ? "승리!" : "패배";
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
