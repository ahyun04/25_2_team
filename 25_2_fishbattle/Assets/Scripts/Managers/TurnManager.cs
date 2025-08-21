using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : SingletonMono<TurnManager>
{
    #region 레퍼런스
    protected override bool DontDestroy => true;

    public enum TeamTurn { Player, Enemy }

    public TeamTurn CurrentTurn { get; private set; } = TeamTurn.Player;

    public bool IsGameStarted { get; private set; } = false;

    public int PlayerAP { get; private set; }
    public int EnemyAP { get; private set; }

    [Header("턴 설정")]
    [SerializeField] private int startAP = 3;
    [SerializeField] private int maxAP = 5;
    private int turnCount = 0;

    private CardManager _cardManager;

    #endregion

    #region 초기화
    protected override void Awake()
    {
        base.Awake();
        _cardManager = FindObjectOfType<CardManager>();
    }

    private void Start()
    {
        StartCoroutine(InitializeGame());
    }

    private IEnumerator InitializeGame()
    {
        // CardManager가 Start에서 덱 셔플 및 초기 드로우를 실행하므로 잠시 대기
        yield return new WaitForSeconds(0.1f);

        // 1. 덱 셔플 및 초기 핸드 드로우 (5장)
        _cardManager.ShuffleDecks();
        _cardManager.DrawStartingHands();

        // 초기 드로우 애니메이션이 끝날 때까지 대기
        yield return new WaitForSeconds(_cardManager.DrawAnimDuration * 5f);

        // 2. 적 AI는 카드 1장을 자동으로 배치 (AP 소모 없음)
        _cardManager.SetupInitialEnemyCard();
        yield return new WaitForSeconds(0.3f);

        // 3. 플레이어의 초기 카드 배치까지 대기
        Debug.Log("게임 시작! 플레이어는 첫 카드를 배치해주세요.");
        yield return new WaitUntil(() => _cardManager.IsPlayerInitialCardPlaced());

        // 4. 게임 시작 AP 설정 및 플레이어 턴 시작
        PlayerAP = startAP;
        EnemyAP = startAP;
        _cardManager.UpdateAPText();

        IsGameStarted = true; // 게임 시작 상태를 true로 설정

        StartPlayerTurn();
    }

    #endregion

    #region 턴
    public void StartPlayerTurn()
    {
        CurrentTurn = TeamTurn.Player;
        turnCount++;

        if (turnCount > 2)
            PlayerAP = Mathf.Min(PlayerAP + 1, maxAP);

        _cardManager.OnTurnStart(true);
    }

    public void StartEnemyTurn()
    {
        CurrentTurn = TeamTurn.Enemy;
        turnCount++;

        if (turnCount > 2)
            EnemyAP = Mathf.Min(EnemyAP + 1, maxAP);

        _cardManager.OnTurnStart(false);

        // AI 실행
        StartCoroutine(_cardManager.EnemyTurnRoutine());
    }

    #endregion

    #region 행동력AP
    /// <summary> 주어진 비용만큼 해당 팀의 AP를 사용 시도합니다. 사용 가능하면 true 반환 </summary>
    public bool SpendAP(bool isPlayer, int cost)
    {
        if (cost < 0)
        {
            Debug.LogError("SpendAP 메서드에는 음수 비용을 사용할 수 없습니다. AP를 환불하려면 AddAP를 사용하세요.");
            return false;
        }

        if (isPlayer)
        {
            if (PlayerAP >= cost)
            {
                PlayerAP -= cost;
                return true;
            }
            return false;
        }
        else
        {
            if (EnemyAP >= cost)
            {
                EnemyAP -= cost;
                return true;
            }
            return false;
        }
    }

    /// <summary> 해당 팀의 AP를 추가합니다. </summary>
    public void AddAP(bool isPlayer, int amount)
    {
        if (isPlayer)
            PlayerAP = Mathf.Min(PlayerAP + amount, maxAP);
        else
            EnemyAP = Mathf.Min(EnemyAP + amount, maxAP);

        _cardManager.UpdateAPText();
    }

    #endregion

    #region 테스트용
    public void OnAttackButtonClicked()
    {
        if (CurrentTurn != TeamTurn.Player)
        {
            Debug.Log("지금은 플레이어 턴이 아닙니다.");
            return;
        }

        // CardManager의 공격 코루틴을 실행합니다.
        StartCoroutine(_cardManager.PlayerAttackRoutine());
    }

    /// <summary> 턐 종료(외부 버튼) — 플레이어가 종료하면 적 턴 시작, 적이 종료하면 플레이어 턴 시작 </summary>
    public void EndTurn()
    {
        if (CurrentTurn == TeamTurn.Player)
        {
            Debug.Log("플레이어 턴 종료 -> 적 턴 시작");
            StartEnemyTurn();
        }
        else
        {
            Debug.Log("적 턴 종료 -> 플레이어 턴 시작");
            StartPlayerTurn();
        }
    }

    #endregion
}
