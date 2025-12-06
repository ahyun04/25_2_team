using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardActionHandler : MonoBehaviour
{
    #region 레퍼런스
    [Header("레퍼런스")]
    private CardManager _cardManager; // 자신에게 데이터를 제공할 CardManager 참조
    private UIManager _uiManager;

    #endregion

    #region 초기화
    private void Start()
    {
        _uiManager = FindObjectOfType<UIManager>();
    }

    public void Setup(CardManager cardManager)
    {
        _cardManager = cardManager;
    }

    #endregion

    #region 플레이어 카드 배치
    // 플레이어가 손패의 카드 인덱스를 배치 합니다.
    public bool PlayCardFromHand(bool isPlayer, int handIndex)
    {
        var hand = isPlayer ? _cardManager._playerHandCards : _cardManager._enemyHandCards;
        var handObjs = isPlayer ? _cardManager.playerCardObjects : _cardManager.enemyCardObjects;

        if (handIndex < 0 || handIndex >= hand.Count) return false;

        FishSO card = hand[handIndex];

        // AP 확인
        if (!TurnManager.Instance.SpendAP(isPlayer, card.AbilityToAct))
        {
            Debug.Log("AP 부족으로 카드를 배치할 수 없습니다.");
            return false;
        }

        var allPlayAreas = isPlayer ?
            _cardManager.playerBattleAreas.Cast<CardSlotArea>().Concat(_cardManager.playerBenchAreas.Cast<CardSlotArea>()) :
            _cardManager.enemyBattleAreas.Cast<CardSlotArea>().Concat(_cardManager.enemyBenchAreas.Cast<CardSlotArea>());

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

        Quaternion rotation;
        Quaternion battleRotation = isPlayer ? Quaternion.Euler(-90, 0, 90) : Quaternion.Euler(90, 0, 90);
        Quaternion benchRotation = isPlayer ? Quaternion.Euler(-90, 0, 90) : Quaternion.Euler(90, 0, 90);

        if (chosenArea is BattlePos)
        {
            rotation = battleRotation;
        }
        else // BenchPos에 배치되는 경우
        {
            rotation = benchRotation;
        }

        GameObject newFishObject = Instantiate(fishPrefab, chosenSlot.position, rotation);
        newFishObject.transform.SetParent(chosenArea.transform);
        newFishObject.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

        if (newFishObject.TryGetComponent<Animator>(out var anim)) 
            anim.enabled = false;

        if (newFishObject.TryGetComponent<CardDisplay>(out var disp))
            disp.SetupCard(card);

        if (newFishObject.TryGetComponent<FishUnit>(out var fishUnit))
        {
            fishUnit.Setup(card, isPlayer); // isPlayer 인자를 전달
        }

        newFishObject.tag = isPlayer ? "PlayerCard" : "EnemyCard";

        // 4. 생성된 물고기를 슬롯에 할당
        chosenArea.OccupySlot(chosenSlot, newFishObject);

        RearrangeHand(isPlayer); // 손패 재정렬

        _uiManager.UpdateAPText();

        return true;
}

    // 손패를 재정렬하고 인덱스를 업데이트
    public void RearrangeHand(bool isPlayer)
    {
        var cardObjects = isPlayer ? _cardManager.playerCardObjects : _cardManager.enemyCardObjects;
        var handPos = isPlayer ? _cardManager.playerHandPosition : _cardManager.enemyHandPosition;

        // 플레이어 핸드의 모든 카드 오브젝트에 대해 반복
        if (isPlayer)
        {
            const int RENDER_LAYER = 9;

            for (int i = 0; i < cardObjects.Count; i++)
            {
                if (cardObjects[i].TryGetComponent<CardDisplay>(out var disp))
                {
                    disp.cardIndex = i;
                    // 플레이어 핸드에 있는 물고기 레이어를 9번으로 변경
                    cardObjects[i].layer = RENDER_LAYER;
                }
            }
        }
        else // 적 핸드 카드일 경우 기존 로직 유지
        {
            for (int i = 0; i < cardObjects.Count; i++)
            {
                if (cardObjects[i].TryGetComponent<CardDisplay>(out var disp))
                {
                    disp.cardIndex = i;
                }
            }
        }

        float totalWidth = (cardObjects.Count - 1) * _cardManager.cardSpacing;
        Vector3 startPos = handPos.position - new Vector3(totalWidth / 2f, 0, 0);

        Quaternion targetRotation = isPlayer ? Quaternion.Euler(-90, -90, 0) : Quaternion.Euler(90, 90, -180);

        for (int i = 0; i < cardObjects.Count; i++)
        {
            Vector3 targetPos = startPos + new Vector3(i * _cardManager.cardSpacing, 0, 0);
            cardObjects[i].transform.DOMove(targetPos, 0.2f).SetEase(Ease.OutCubic);
        }
    }

    #endregion
}
