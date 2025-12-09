using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class CardManager : SingletonMono<CardManager>
{
    #region 레퍼런스
    protected override bool DontDestroy => false;
    private CardActionHandler _ActionHandler;
    private PlayerHandController _PlayerHandController;

    [Header("인벤토리 레퍼런스")]
    private InventoryHolder _playerInventory;

    [Header("플레이어 카드 데이터")]
    [SerializeField] private List<FishSO> _playerDeckCards = new List<FishSO>();
    public List<FishSO> _playerHandCards = new List<FishSO>();

    [Header("적 카드 데이터")]
    [SerializeField] private List<FishSO> _enemyDeckCards = new List<FishSO>();
    public List<FishSO> _enemyHandCards = new List<FishSO>();

    [Header("플레이어 위치")]
    [SerializeField] private Transform _playerDeckPosition;
    public Transform playerHandPosition;
    public List<GameObject> playerCardObjects = new List<GameObject>();

    [Header("적 위치")]
    [SerializeField] private Transform _enemyDeckPosition;
    public Transform enemyHandPosition;
    public List<GameObject> enemyCardObjects = new List<GameObject>();

    [Header("배틀 슬롯 참조 (씬에 배치된 BattlePos들)")]
    public BattlePos[] playerBattleAreas;
    public BattlePos[] enemyBattleAreas;
    public BenchPos[] playerBenchAreas;
    public BenchPos[] enemyBenchAreas;

    [Header("카드 조정")]
    public float cardSpacing = 2f;
    [SerializeField] private int maxHandCards = 5;
    [SerializeField] private float drawAnimDuration = 0.4f;

    public float DrawAnimDuration => drawAnimDuration;

    private bool _isPlayerInitialCardPlaced = false;
    public bool IsPlayerInitialCardPlaced() => _isPlayerInitialCardPlaced;

    #endregion

    #region 초기화
    protected override void Awake()
    {
        base.Awake();

        _ActionHandler = FindObjectOfType<CardActionHandler>();
        if (_ActionHandler == null)
        {
            _ActionHandler = gameObject.AddComponent<CardActionHandler>();
        }

        _ActionHandler.Setup(this);
    }

    private void Start()
    {
        _playerInventory = FindObjectOfType<InventoryHolder>();
        _PlayerHandController = GetComponent<PlayerHandController>();

        SetDeckFromInventory();
    }
    
    private void SetDeckFromInventory()
    {
        // 기존에 미리 설정된 덱을 사용하지 않으므로, 덱을 비워줍니다.
        _playerDeckCards.Clear();

        if (_playerInventory == null)
        {
            Debug.LogError("플레이어 인벤토리 레퍼런스가 없습니다.");
            return;
        }

        // 인벤토리 슬롯들을 순회하며 덱을 구성합니다.
        foreach (var slot in _playerInventory.InventorySystem.InventorySlots)
        {
            // 슬롯에 아이템이 존재할 때만 처리합니다.
            if (slot.ItemData != null)
            {
                for (int i = 0; i < slot.StackSize; i++)
                {
                    // 물고기 데이터가 카드 데이터 역할을 한다고 가정
                    _playerDeckCards.Add(slot.ItemData);
                }
            }
        }

        // 인벤토리의 물고기로 덱을 구성한 후, 섞기 및 드로우 로직을 호출합니다.
        ShuffleDecks();
        DrawStartingHands();
    }

    // 덱을 섞습니다
    public void ShuffleDecks()
    {
        ShuffleDeck(_playerDeckCards);
        ShuffleDeck(_enemyDeckCards);
    }

    // 게임 시작 시 양 팀의 초기 손패를 드로우합니다
    public void DrawStartingHands()
    {
        for (int i = 0; i < maxHandCards; i++)
        {
            DrawOne(true, false);
            DrawOne(false, false);
        }
    }

    // 적 AI만 게임 시작 시 AP 소모 없이 배틀 필드에 1장 놓는 초기 세팅.
    public void SetupInitialEnemyCard()
    {
        if (_enemyHandCards.Count > 0)
        {
            PlayCardObjectDirectly(false, 0);
            Debug.Log("적 AI가 첫 카드를 배치했습니다.");
        }
    }

    // 플레이어가 수동으로 놓는 초기 카드 배치 로직 (AP 소모 없음).
    public void SetupInitialPlayerCard(int handIndex)
    {
        PlayCardObjectDirectly(true, handIndex);
        _isPlayerInitialCardPlaced = true;
    }

    // AP 소모 없이 카드를 직접 배틀 필드에 배치하는 내부 헬퍼 메서드
    private void PlayCardObjectDirectly(bool isPlayer, int handIndex)
    {
        var hand = isPlayer ? _playerHandCards : _enemyHandCards;
        var handObjs = isPlayer ? playerCardObjects : enemyCardObjects;
        //var battleAreas = isPlayer ? playerBattleAreas : enemyBattleAreas;

        if (handIndex < 0 || handIndex >= hand.Count) return;

        FishSO card = hand[handIndex];

        bool isBenchOnly = card.Position == Position.Defence ||
                       card.Position == Position.Heal ||
                       card.Position == Position.Support;

        CardSlotArea chosenArea = null;
        Transform chosenSlot = null;

        // 아까 수정한 배열 선언
        CardSlotArea[] targetAreas = isBenchOnly
            ? (CardSlotArea[])(isPlayer ? playerBenchAreas : enemyBenchAreas)
            : (CardSlotArea[])(isPlayer ? playerBattleAreas : enemyBattleAreas);

        foreach (var area in targetAreas)
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
            string zoneName = isBenchOnly ? "벤치" : "배틀 필드";
            Debug.Log($"빈 {zoneName} 슬롯이 없어 카드를 배치할 수 없습니다: {card.Name} ({card.Position})");
            return;
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
            _ActionHandler.RearrangeHand(isPlayer); // 손패 재정렬
            return;
        }

        // 물고기 생성될때 플레이어/적 rotation값 지정
        Quaternion rotation = isPlayer ? Quaternion.Euler(-90, 0, -90) : Quaternion.Euler(90, 0, 90);

        GameObject newFishObject = Instantiate(fishPrefab, chosenSlot.position, rotation);
        newFishObject.transform.SetParent(chosenArea.transform);
        newFishObject.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

        if (newFishObject.TryGetComponent<Animator>(out var anim))
        {
            anim.enabled = false;
        }

        if (newFishObject.TryGetComponent<CardDisplay>(out var disp))
        {
            disp.SetupCard(card);
        }

        if (newFishObject.TryGetComponent<FishUnit>(out var fishUnit))
        {
            fishUnit.Setup(card, isPlayer);
        }

        newFishObject.tag = isPlayer ? "PlayerCard" : "EnemyCard";

        // 4. 생성된 물고기를 슬롯에 할당
        chosenArea.OccupySlot(chosenSlot, newFishObject);

        _ActionHandler.RearrangeHand(isPlayer);

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

    // 실제 드로우 로직. isAnimated true이면 DOTween 애니메이션 사용
    public void DrawOne(bool isPlayer, bool isAnimated)
    {
        var deck = isPlayer ? _playerDeckCards : _enemyDeckCards;
        var hand = isPlayer ? _playerHandCards : _enemyHandCards;
        var cardObjects = isPlayer ? playerCardObjects : enemyCardObjects;
        var deckPos = isPlayer ? _playerDeckPosition : _enemyDeckPosition;
        var handPos = isPlayer ? playerHandPosition : enemyHandPosition;

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
        Quaternion rotation = isPlayer ? Quaternion.Euler(-90, -90, 0) : Quaternion.Euler(90, 90, -180);

        GameObject fishObject = Instantiate(cd.Prefab, deckPos.position, rotation);
        fishObject.tag = isPlayer ? "PlayerCard" : "EnemyCard";

        if (isPlayer)
        {
            _PlayerHandController.SetOriginalCardScale(fishObject.transform.localScale);
            fishObject.layer = 9;
        }

        fishObject.transform.SetParent(handPos, true);
        fishObject.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

        if (fishObject.TryGetComponent<Animator>(out var anim))
        {
            anim.enabled = false;
        }

        // 생성된 물고기 오브젝트에 있는 CardDisplay 컴포넌트에 데이터 전달
        if (fishObject.TryGetComponent<CardDisplay>(out var disp))
        {
            disp.SetupCard(cd);
            disp.cardIndex = hand.Count - 1;
        }

        cardObjects.Add(fishObject);

        // 카드를 먼저 손패의 중앙으로 이동시킨다.
        if (isAnimated)
        {
            fishObject.transform.DOMove(handPos.position, drawAnimDuration)
                .SetEase(Ease.OutCubic)
                .OnComplete(() => _ActionHandler.RearrangeHand(isPlayer)); // 이동 완료 후 손패 전체를 재정렬
        }
        else
        {
            fishObject.transform.position = handPos.position;
            _ActionHandler.RearrangeHand(isPlayer); // 애니메이션 없으면 바로 재정렬
        }
    }

    #endregion

    #region 턴 시작 시 카드매니저 콜백
    // TurnManager가 턴을 시작할 때 호출합니다. isPlayer 여부로 드로우 처리
    public void OnTurnStart(bool isPlayer)
    {
        DrawOne(isPlayer, true);
    }
    #endregion

    #region 밴치 -> 배틀필드로 옮기는 기능
    public void SetBenchCardLayers(bool isPlayer, int layer)
    {
        var benchAreas = isPlayer ? playerBenchAreas : enemyBenchAreas;

        foreach (var benchArea in benchAreas)
        {
            foreach (var cardObj in benchArea.GetOccupiedCards())
            {
                if (cardObj != null)
                {
                    if (cardObj.TryGetComponent<FishUnit>(out var unit) && unit.CardData.Position == Position.Heal)
                    {
                        cardObj.layer = 9;
                    }
                    else
                    {
                        cardObj.layer = layer;
                    }
                }
            }
        }
    }

    #endregion

    #region 룰 관련 헬퍼 메서드

    // 아군 벤치에 살아있는 탱커가 있는지 확인하고 반환
    public FishUnit GetBenchTanker(bool isPlayer)
    {
        var benchAreas = isPlayer ? playerBenchAreas : enemyBenchAreas;

        // 벤치 슬롯을 순회하며 유닛을 찾음
        foreach (var area in benchAreas)
        {
            var cards = area.GetOccupiedCards();
            foreach (var cardObj in cards)
            {
                if (cardObj != null && cardObj.TryGetComponent<FishUnit>(out var unit))
                {
                    // 죽지 않았고, 포지션이 Tanker인 경우
                    if (!unit.IsDead && unit.CardData.Position == Position.Defence)
                    {
                        return unit;
                    }
                }
            }
        }
        return null;
    }

    public FishUnit GetMostInjuredBattleUnit(bool isPlayer)
    {
        var battleAreas = isPlayer ? playerBattleAreas : enemyBattleAreas;
        FishUnit mostInjuredUnit = null;
        int lowestHp = int.MaxValue;

        foreach (var area in battleAreas)
        {
            // 부모 클래스(CardSlotArea)의 GetOccupiedCards 사용
            foreach (var cardObj in area.GetOccupiedCards())
            {
                if (cardObj != null && cardObj.TryGetComponent<FishUnit>(out var unit))
                {
                    // 1. 이미 죽은 유닛 제외
                    if (unit.IsDead) continue;

                    // 2. 풀피(Full HP)인 유닛 제외 (치료할 필요 없음)
                    if (unit.CurrentHp >= unit.CardData.Hp) continue;

                    // 3. 현재 체력이 가장 낮은 유닛을 찾음 (위급한 순서)
                    if (unit.CurrentHp < lowestHp)
                    {
                        lowestHp = unit.CurrentHp;
                        mostInjuredUnit = unit;
                    }
                }
            }
        }

        return mostInjuredUnit;
    }

    #endregion
}