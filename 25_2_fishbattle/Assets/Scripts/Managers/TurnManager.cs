using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnManager : SingletonMono<TurnManager>
{
    #region 레퍼런스
    protected override bool DontDestroy => false;

    public enum TeamTurn { Player, Enemy }

    public TeamTurn CurrentTurn { get; private set; } = TeamTurn.Player;

    public bool IsGameStarted { get; private set; } = false;

    public int PlayerAP { get; private set; }
    public int EnemyAP { get; private set; }

    public int PlayerKillCount { get; private set; }
    public int EnemyKillCount { get; private set; }

    [Header("턴 설정")]
    [SerializeField] private int startAP = 3;
    [SerializeField] private int maxAP = 5;
    private int turnCount = 0;

    private bool _isSwitchingTurns = false;

    private CardManager _cardManager;
    private EnemyAI_Controller _enemyAIController;
    private UIManager _uiManager;

    #endregion

    #region 초기화
    protected override void Awake()
    {
        base.Awake();
        _cardManager = FindObjectOfType<CardManager>();
        _enemyAIController = FindObjectOfType<EnemyAI_Controller>();
        _uiManager = FindObjectOfType<UIManager>();

        // 게임 시작 시 킬 카운트 초기화
        PlayerKillCount = 0;
        EnemyKillCount = 0;
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
        _uiManager.UpdateAPText();

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

        _isSwitchingTurns = false;
        if (_uiManager != null)
        {
            _uiManager.SetButtonUIsForPlayerTurn(true);
        }
    }

    public void StartEnemyTurn()
    {
        CurrentTurn = TeamTurn.Enemy;
        turnCount++;

        if (turnCount > 2)
            EnemyAP = Mathf.Min(EnemyAP + 1, maxAP);

        _cardManager.OnTurnStart(false);

        if (_uiManager != null)
        {
            _uiManager.SetButtonUIsForPlayerTurn(false); // 적 턴이므로 비활성화 (잠금)
        }

        // AI 실행
        _enemyAIController.ExecuteTurn(); 
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

        _uiManager.UpdateAPText();
    }

    #endregion

    #region 공격 처리
    // 배틀에 올라간 해당 팀의 카드 오브젝트들로 공격 시뮬레이션
    public IEnumerator PlayerAttackRoutine()
    {
        var playerBattleCards = new List<GameObject>();
        foreach (var area in CardManager.Instance.playerBattleAreas)
        {
            playerBattleCards.AddRange(area.GetOccupiedCards());
        }

        var enemyBattleCards = new List<GameObject>();
        foreach (var area in CardManager.Instance.enemyBattleAreas)
        {
            enemyBattleCards.AddRange(area.GetOccupiedCards());
        }

        // 공격할 유닛이 하나라도 있으면 공격 페이즈 진행
        foreach (var attackerObj in playerBattleCards)
        {
            // 공격자 또는 공격자 유닛 정보가 없으면 건너뛰기
            if (attackerObj == null || !attackerObj.TryGetComponent<FishUnit>(out var attackerUnit))
            {
                continue;
            }

            // 공격에 필요한 AP(AbilityToAct)가 있는지 확인
            if (!TurnManager.Instance.SpendAP(true, attackerUnit.CardData.AbilityToAct))
            {
                Debug.Log($"{attackerUnit.CardData.Name}은(는) AP가 부족하여 공격할 수 없습니다.");
                continue; // AP가 부족하면 다음 유닛으로 넘어감
            }

            // 살아있는 적 유닛 리스트를 필터링
            var livingEnemies = enemyBattleCards.Where(e => e != null).ToList();
            if (livingEnemies.Count == 0)
            {
                Debug.Log("공격할 적이 없습니다.");
                TurnManager.Instance.AddAP(true, attackerUnit.CardData.AbilityToAct);
                break;
            }

            // 간단한 타겟팅: 첫 번째 적을 공격
            GameObject targetObj = livingEnemies[0];
            if (targetObj.TryGetComponent<FishUnit>(out var targetUnit))
            {
                Debug.Log($"{attackerUnit.CardData.Name}이(가) {targetUnit.CardData.Name}을(를) 공격! (소모 AP: {attackerUnit.CardData.AbilityToAct})");
                targetUnit.TakeDamage(attackerUnit.CardData.Damage);
            }

            yield return new WaitForSeconds(0.5f); // 공격 사이의 딜레이
        }
    }

    public void CheckBattlefieldAndEnableBenchDrag()
    {
        bool playerHasEmptySlot = false;
        foreach (var area in CardManager.Instance.playerBattleAreas)
        {
            foreach (var slot in area.slots)
            {
                if (!area.IsSlotOccupied(slot))
                {
                    playerHasEmptySlot = true;
                    break;
                }
            }
            if (playerHasEmptySlot) break;
        }

        if (playerHasEmptySlot)
        {
            Debug.Log("플레이어 배틀필드에 빈자리가 생겼습니다. 벤치 카드를 이동할 수 있습니다.");
            CardManager.Instance.SetBenchCardLayers(true, 9); // 플레이어 벤치 카드 레이어를 9번으로 설정
        }
        else
        {
            CardManager.Instance.SetBenchCardLayers(true, 0); // 빈자리가 없으면 다시 0번으로 복구
        }
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

        if (_isSwitchingTurns)
        {
            Debug.Log("턴 전환 중에는 공격할 수 없습니다.");
            return;
        }

        // CardManager의 공격 코루틴을 실행합니다.
        StartCoroutine(PlayerAttackRoutine());
    }

    public void EndTurn()
    {
        if (CurrentTurn == TeamTurn.Player)
        {
            if (_isSwitchingTurns) return;
            _isSwitchingTurns = true;

            Debug.Log("플레이어 턴 종료 -> 적 턴 시작");
            StartEnemyTurn();
        }
        else
        {
            Debug.Log("적 턴 종료 -> 플레이어 턴 시작");
            StartPlayerTurn();
        }
    }

    public void AddKill(bool isPlayer)
    {
        if (isPlayer)
        {
            PlayerKillCount++;
        }
        else
        {
            EnemyKillCount++;
        }

        _uiManager.UpdateKillCountText(); // CardManager의 UI 업데이트 메서드 호출
    }

    #endregion
}
