using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardManager : SingletonMono<CardManager>
{
    #region 레퍼런스
    protected override bool DontDestroy => false;

    [Header("플레이어 카드 데이터")]
    [SerializeField] private List<FishSO> _playerDeckCards = new List<FishSO>();
    public List<FishSO> _playerHandCards = new List<FishSO>();

    [Header("적 카드 데이터")]
    [SerializeField] private List<FishSO> _enemyDeckCards = new List<FishSO>();
    [SerializeField] private List<FishSO> _enemyHandCards = new List<FishSO>();

    [Header("플레이어 위치")]
    [SerializeField] private Transform _playerDeckPosition;
    [SerializeField] private Transform _playerHandPosition;
    private List<GameObject> _playerCardObjects = new List<GameObject>();

    [Header("적 위치")]
    [SerializeField] private Transform _enemyDeckPosition;
    [SerializeField] private Transform _enemyHandPosition;
    private List<GameObject> _enemyCardObjects = new List<GameObject>();

    [Header("배틀 슬롯 참조 (씬에 배치된 BattlePos들)")]
    [SerializeField] private BattlePos[] _playerBattleAreas;
    [SerializeField] private BattlePos[] _enemyBattleAreas;
    [SerializeField] private BenchPos[] _playerBenchAreas;
    [SerializeField] private BenchPos[] _enemyBenchAreas;

    [Header("카드 조정")]
    [SerializeField] private float cardSpacing = 2f;
    [SerializeField] private int maxHandCards = 5;
    [SerializeField] private float drawAnimDuration = 0.4f;

    public float DrawAnimDuration => drawAnimDuration;

    private bool _isPlayerInitialCardPlaced = false;
    public bool IsPlayerInitialCardPlaced() => _isPlayerInitialCardPlaced;

    #endregion

    #region 초기화
    /// <summary> 덱을 섞습니다. </summary>
    public void ShuffleDecks()
    {
        ShuffleDeck(_playerDeckCards);
        ShuffleDeck(_enemyDeckCards);
    }

    /// <summary> 게임 시작 시 양 팀의 초기 손패를 드로우합니다. </summary>
    public void DrawStartingHands()
    {
        for (int i = 0; i < maxHandCards; i++)
        {
            DrawOne(true, false);
            DrawOne(false, false);
        }
    }

    /// <summary>
    /// 적 AI만 게임 시작 시 AP 소모 없이 배틀 필드에 1장 놓는 초기 세팅.
    /// </summary>
    public void SetupInitialEnemyCard()
    {
        if (_enemyHandCards.Count > 0)
        {
            PlayCardObjectDirectly(false, 0);
            Debug.Log("적 AI가 첫 카드를 배치했습니다.");
        }
    }

    /// <summary>
    /// 플레이어가 수동으로 놓는 초기 카드 배치 로직 (AP 소모 없음).
    /// </summary>
    public void SetupInitialPlayerCard(int handIndex)
    {
        PlayCardObjectDirectly(true, handIndex);
        _isPlayerInitialCardPlaced = true;
    }

    /// <summary> AP 소모 없이 카드를 직접 배틀 필드에 배치하는 내부 헬퍼 메서드 </summary>
    private void PlayCardObjectDirectly(bool isPlayer, int handIndex)
    {
        var hand = isPlayer ? _playerHandCards : _enemyHandCards;
        var handObjs = isPlayer ? _playerCardObjects : _enemyCardObjects;
        var battleAreas = isPlayer ? _playerBattleAreas : _enemyBattleAreas;

        if (handIndex < 0 || handIndex >= hand.Count) return;

        Transform chosenSlot = null;
        BattlePos chosenArea = null;
        foreach (var area in battleAreas)
        {
            var slot = area.GetNearestEmptySlot(Vector3.zero);
            if (slot != null)
            {
                chosenSlot = slot;
                chosenArea = area;
                break;
            }
        }

        if (chosenSlot == null)
        {
            Debug.Log("빈 배틀 슬롯이 없어 초기 카드 배치를 할 수 없습니다.");
            return;
        }

        FishSO card = hand[handIndex];
        GameObject cardObj = handObjs[handIndex];

        // 1. 손패에서 해당 카드 데이터 및 오브젝트 제거
        hand.RemoveAt(handIndex);
        handObjs.RemoveAt(handIndex);

        // 2. 카드 UI 오브젝트 파괴
        Destroy(cardObj);

        // 3. FishSO에 연결된 프리팹을 생성
        GameObject fishPrefab = card.Prefab;
        if (fishPrefab == null)
        {
            Debug.LogError($"FishSO '{card.Name}'에 프리팹이 연결되지 않았습니다!");
            RearrangeHand(isPlayer); // 손패 재정렬
            return;
        }

        // 물고기 생성될때 플레이어/적 rotation값 지정
        Quaternion rotation = isPlayer ? Quaternion.Euler(0, 90, 180) : Quaternion.Euler(0, 90, 0);

        GameObject newFishObject = Instantiate(fishPrefab, chosenSlot.position, rotation);
        newFishObject.transform.SetParent(chosenArea.transform);

        if (newFishObject.TryGetComponent<CardDisplay>(out var disp))
        {
            disp.SetupCard(card);
        }

        newFishObject.tag = isPlayer ? "PlayerCard" : "EnemyCard";

        // 4. 생성된 물고기를 슬롯에 할당
        chosenArea.OccupySlot(chosenSlot, newFishObject);

        RearrangeHand(isPlayer);

        Debug.Log($"초기 카드 배치 - {(isPlayer ? "플레이어" : "적")}의 카드가 배틀 필드에 놓였습니다.");
    }

    #endregion

    #region 덱 / 드로우
    private void ShuffleDeck(List<FishSO> deck)
    {
        List<FishSO> temp = new List<FishSO>(deck);
        deck.Clear();
        while (temp.Count > 0)
        {
            int r = Random.Range(0, temp.Count);
            deck.Add(temp[r]);
            temp.RemoveAt(r);
        }
    }

    /// <summary>
    /// 실제 드로우 로직. isAnimated true이면 DOTween 애니메이션 사용
    /// </summary>
    public void DrawOne(bool isPlayer, bool isAnimated)
    {
        var deck = isPlayer ? _playerDeckCards : _enemyDeckCards;
        var hand = isPlayer ? _playerHandCards : _enemyHandCards;
        var cardObjects = isPlayer ? _playerCardObjects : _enemyCardObjects;
        var deckPos = isPlayer ? _playerDeckPosition : _enemyDeckPosition;
        var handPos = isPlayer ? _playerHandPosition : _enemyHandPosition;

        if (hand.Count >= maxHandCards)
        {
            Debug.Log($"{(isPlayer ? "플레이어" : "적")} 손패가 가득 찼습니다.");
            return;
        }

        if (deck.Count == 0)
        {
            Debug.Log($"{(isPlayer ? "플레이어" : "적")} 덱이 비었습니다.");
            return;
        }

        FishSO cd = deck[0];
        deck.RemoveAt(0);
        hand.Add(cd);

        if (cd.Prefab == null)
        {
            Debug.LogError($"FishSO '{cd.Name}'에 물고기 프리팹이 연결되지 않았습니다!");
            return;
        }

        // 물고기 프리팹을 Instantiate
        Quaternion rotation = isPlayer ? Quaternion.Euler(0, 90, 180) : Quaternion.Euler(0, 90, 0);

        GameObject fishObject = Instantiate(cd.Prefab, deckPos.position, rotation);
        fishObject.tag = isPlayer ? "PlayerCard" : "EnemyCard";

        fishObject.transform.SetParent(handPos, true);

        // 생성된 물고기 오브젝트에 있는 CardDisplay 컴포넌트에 데이터 전달
        if (fishObject.TryGetComponent<CardDisplay>(out var disp))
        {
            disp.SetupCard(cd);
            disp.cardIndex = hand.Count - 1;
        }

        cardObjects.Add(fishObject);

        // 목표 위치 중앙정렬 계산
        float totalWidth = (maxHandCards - 1) * cardSpacing;
        float startX = -totalWidth / 2f;
        Vector3 targetPos = handPos.position + new Vector3(startX + (hand.Count - 1) * cardSpacing, 0, 0);

        if (isAnimated)
            fishObject.transform.DOMove(targetPos, drawAnimDuration).SetEase(Ease.OutCubic);
        else
            fishObject.transform.position = targetPos;
    }

    #endregion

    #region 플레이어/적 카드 배치(Hand -> Battle)
    /// <summary>
    /// 플레이어(또는 적)가 손패의 카드 인덱스를 배치(플레이)합니다.
    /// 성공하면 true 반환 (AP 부족/빈 슬롯 없음 등 실패시 false)
    /// </summary>
    public bool PlayCardFromHand(bool isPlayer, int handIndex)
    {
        var hand = isPlayer ? _playerHandCards : _enemyHandCards;
        var handObjs = isPlayer ? _playerCardObjects : _enemyCardObjects;

        if (handIndex < 0 || handIndex >= hand.Count) return false;

        FishSO card = hand[handIndex];

        // AP 확인
        if (!TurnManager.Instance.SpendAP(isPlayer, card.AbilityToAct))
        {
            Debug.Log("AP 부족으로 카드를 배치할 수 없습니다.");
            return false;
        }

        var allPlayAreas = isPlayer ?
            _playerBattleAreas.Cast<CardSlotArea>().Concat(_playerBenchAreas.Cast<CardSlotArea>()) :
            _enemyBattleAreas.Cast<CardSlotArea>().Concat(_enemyBenchAreas.Cast<CardSlotArea>());

        Transform chosenSlot = null;
        CardSlotArea chosenArea = null;

        foreach (var area in allPlayAreas)
        {
            var slot = area.GetNearestEmptySlot(Vector3.zero);
            if (slot != null)
            {
                chosenSlot = slot;
                chosenArea = area;
                break;
            }
        }

        if (chosenSlot == null)
        {
            Debug.Log("빈 슬롯이 없습니다.");
            TurnManager.Instance.AddAP(isPlayer, card.AbilityToAct);
            return false;
        }

        GameObject cardObj = handObjs[handIndex];

        // 1. 손패에서 해당 카드 데이터 및 오브젝트 제거
        hand.RemoveAt(handIndex);
        handObjs.RemoveAt(handIndex);

        // 2. 카드 UI 오브젝트 파괴
        Destroy(cardObj);

        // 3. FishSO에 연결된 프리팹을 생성
        GameObject fishPrefab = card.Prefab;
        if (fishPrefab == null)
        {
            Debug.LogError($"FishSO '{card.Name}'에 프리팹이 연결되지 않았습니다!");
            TurnManager.Instance.AddAP(isPlayer, card.AbilityToAct);
            RearrangeHand(isPlayer);
            return false;
        }

        Quaternion rotation = isPlayer ? Quaternion.Euler(0, 90, 180) : Quaternion.Euler(0, 90, 0);

        GameObject newFishObject = Instantiate(fishPrefab, chosenSlot.position, rotation);
        newFishObject.transform.SetParent(chosenArea.transform);

        if (newFishObject.TryGetComponent<CardDisplay>(out var disp))
            disp.SetupCard(card);

        newFishObject.tag = isPlayer ? "PlayerCard" : "EnemyCard";

        // 4. 생성된 물고기를 슬롯에 할당
        chosenArea.OccupySlot(chosenSlot, newFishObject);

        RearrangeHand(isPlayer); // 손패 재정렬

        Debug.Log($"{(isPlayer ? "플레이어" : "적")}이 카드를 배치했습니다: {card.Name} (남은 AP:{(isPlayer ? TurnManager.Instance.PlayerAP : TurnManager.Instance.EnemyAP)})");
        return true;
    }

    /// <summary> 손패를 재정렬하고 인덱스를 업데이트합니다. </summary>
    private void RearrangeHand(bool isPlayer)
    {
        var cardObjects = isPlayer ? _playerCardObjects : _enemyCardObjects;
        var handPos = isPlayer ? _playerHandPosition : _enemyHandPosition;

        for (int i = 0; i < cardObjects.Count; i++)
        {
            if (cardObjects[i].TryGetComponent<CardDisplay>(out var disp))
            {
                disp.cardIndex = i;
            }
        }

        float totalWidth = (cardObjects.Count - 1) * cardSpacing;
        Vector3 startPos = handPos.position - new Vector3(totalWidth / 2f, 0, 0);

        for (int i = 0; i < cardObjects.Count; i++)
        {
            Vector3 targetPos = startPos + new Vector3(i * cardSpacing, 0, 0);
            cardObjects[i].transform.DOMove(targetPos, 0.2f).SetEase(Ease.OutCubic);
        }
}
    #endregion

    #region 간단 공격 처리
    /// <summary> 단순 공격: 배틀에 올라간 해당 팀의 카드 오브젝트들로 공격 시뮬레이션 </summary>
    public IEnumerator PlayerAttackRoutine()
    {
        var playerBattleCards = new List<GameObject>();

        // 플레이어 배틀 슬롯의 카드들을 모두 가져옴
        // 이제 `GetOccupiedCards()` public 메서드를 사용합니다.
        foreach (var area in _playerBattleAreas)
        {
            playerBattleCards.AddRange(area.GetOccupiedCards());
        }

        // 각 카드의 공격을 순차적으로 처리
        foreach (var cardObj in playerBattleCards)
        {
            // 공격 로직 구현 (예시: 상대방에게 데미지 입히기)
            Debug.Log($"플레이어 유닛이 공격을 시작합니다: {cardObj.name}");

            // 여기에 실제 공격 로직(데미지 적용, 애니메이션 등)을 추가할 부분

            yield return new WaitForSeconds(0.5f); // 공격 애니메이션 대기
        }

        Debug.Log("플레이어 공격 페이즈 종료. 턴을 종료합니다.");

        // 공격이 끝나면 강제로 턴 종료
        TurnManager.Instance.EndTurn();
        yield break;
    }

    public IEnumerator EnemyAttackRoutine()
    {
        var areas = _enemyBattleAreas;
        foreach (var area in areas)
        {
            foreach (var slot in area.slots)
            {
                if (area.IsSlotOccupied(slot))
                {
                    if (!TurnManager.Instance.SpendAP(false, 1)) yield break;
                    Debug.Log($"적 유닛이 공격을 실행했습니다. (AP 남음:{TurnManager.Instance.EnemyAP})");
                    yield return new WaitForSeconds(0.35f);
                }
            }
        }
    }
    #endregion

    #region 적 AI 루틴
    /// <summary>
    /// 적 턴 전체 루틴: 드로우는 TurnManager에서 이미 호출됨(카드Manager.OnTurnStart).
    /// </summary>
    public IEnumerator EnemyTurnRoutine()
    {
        bool playedAny = true;
        while (playedAny)
        {
            playedAny = false;
            for (int i = 0; i < _enemyHandCards.Count; i++)
            {
                FishSO c = _enemyHandCards[i];
                if (TurnManager.Instance.EnemyAP >= c.AbilityToAct)
                {
                    bool ok = PlayCardFromHand(false, i);
                    if (ok)
                    {
                        playedAny = true;
                        yield return new WaitForSeconds(0.25f);
                        break;
                    }
                }
            }
        }
        yield return StartCoroutine(EnemyAttackRoutine());
        TurnManager.Instance.EndTurn();
        yield break;
    }
    #endregion

    #region 턴 시작 시 카드매니저 콜백
    /// <summary> TurnManager가 턴을 시작할 때 호출합니다. isPlayer 여부로 드로우 처리 </summary>
    public void OnTurnStart(bool isPlayer)
    {
        DrawOne(isPlayer, true);
    }
    #endregion
}