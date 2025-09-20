using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    #region 레퍼런스
    private CardManager _cardManager;

    #endregion

    #region 초기화
    private void Start()
    {
        _cardManager = FindObjectOfType<CardManager>();
    }

    #endregion

    #region 적 AI 루틴
    // 적 턴 전체 루틴: 드로우는 TurnManager에서 이미 호출됨(카드Manager.OnTurnStart).
    public IEnumerator EnemyTurnRoutine()
    {
        // 적 턴 시작 로그
        Debug.Log($"적 턴 시작. 현재 AP: {TurnManager.Instance.EnemyAP}");

        // 이 턴에 배치할 카드의 수. 필요에 따라 랜덤으로 설정 가능
        int cardsToPlay = 1;

        for (int i = 0; i < cardsToPlay; i++)
        {
            // 손패에 카드가 있는지 확인
            if (_cardManager._enemyHandCards.Count == 0)
            {
                Debug.Log("적 손패에 카드가 없어 카드 배치를 종료합니다.");
                break;
            }

            // 가장 AP 소모가 적은 카드를 먼저 배치
            FishSO cardToPlay = _cardManager._enemyHandCards.OrderBy(c => c.AbilityToAct).FirstOrDefault();

            if (cardToPlay != null && TurnManager.Instance.EnemyAP >= cardToPlay.AbilityToAct)
            {
                int cardIndex = _cardManager._enemyHandCards.IndexOf(cardToPlay);
                bool playedOK = _cardManager.PlayCardFromHand(false, cardIndex);

                if (playedOK)
                {
                    Debug.Log($"적 AI가 카드 [{cardToPlay.Name}]를 배치했습니다.");
                    yield return new WaitForSeconds(0.25f); // 배치 후 약간의 딜레이
                }
                else
                {
                    // 카드 배치에 실패하면 루프 종료
                    Debug.Log("카드 배치 실패로 턴의 카드 배치를 종료합니다.");
                    break;
                }
            }
            else
            {
                // 더 이상 놓을 수 있는 카드가 없으면 루프 종료
                Debug.Log("AP 부족 또는 유효한 카드가 없어 카드 배치를 종료합니다.");
                break;
            }
        }

        yield return StartCoroutine(EnemyAttackRoutine());

        // 공격이 끝난 후 적의 빈 슬롯을 확인하고 자동 교체
        AutoReplaceEnemyUnitFromBench();

        TurnManager.Instance.EndTurn();
        yield break;
    }

    public IEnumerator EnemyAttackRoutine()
    {
        var enemyBattleCards = new List<GameObject>();
        foreach (var area in _cardManager.enemyBattleAreas)
        {
            enemyBattleCards.AddRange(area.GetOccupiedCards());
        }

        var playerBattleCards = new List<GameObject>();
        foreach (var area in _cardManager.playerBattleAreas)
        {
            playerBattleCards.AddRange(area.GetOccupiedCards());
        }

        foreach (var attackerObj in enemyBattleCards)
        {
            if (attackerObj == null || !attackerObj.TryGetComponent<FishUnit>(out var attackerUnit))
            {
                continue;
            }

            // 공격에 필요한 AP(AbilityToAct)가 있는지 확인하고 소모
            if (!TurnManager.Instance.SpendAP(false, attackerUnit.CardData.AbilityToAct))
            {
                Debug.Log($"적 유닛 {attackerUnit.CardData.Name}은(는) AP가 부족하여 공격할 수 없습니다.");
                continue; // AP가 부족하면 다음 유닛으로 넘어감
            }

            var livingPlayers = playerBattleCards.Where(p => p != null).ToList();
            if (livingPlayers.Count == 0)
            {
                Debug.Log("공격할 플레이어 유닛이 없습니다.");
                // AP를 소모했지만 공격 대상이 없으므로 다시 돌려줌 (선택적)
                TurnManager.Instance.AddAP(false, attackerUnit.CardData.AbilityToAct);
                break;
            }

            GameObject targetObj = livingPlayers[0];
            if (targetObj.TryGetComponent<FishUnit>(out var targetUnit))
            {
                Debug.Log($"적 유닛 {attackerUnit.CardData.Name}이(가) {targetUnit.CardData.Name}을(를) 공격! (소모 AP: {attackerUnit.CardData.AbilityToAct}, 남은 AP:{TurnManager.Instance.EnemyAP})");
                targetUnit.TakeDamage(attackerUnit.CardData.Damage);
            }

            yield return new WaitForSeconds(0.35f);
        }
    }

    public void AutoReplaceEnemyUnitFromBench()
    {
        // 적 배틀 슬롯 배열을 순회합니다.
        foreach (var battleArea in _cardManager.enemyBattleAreas)
        {
            foreach (var battleSlot in battleArea.slots)
            {
                // 배틀 슬롯이 비어 있는지 확인합니다.
                if (!battleArea.IsSlotOccupied(battleSlot))
                {
                    // 벤치에서 대체할 유닛을 찾습니다.
                    GameObject unitToMove = null;
                    CardSlotArea sourceArea = null;
                    Transform sourceSlot = null;

                    // 벤치 슬롯 영역을 순회하며 비어있지 않은 첫 번째 슬롯을 찾습니다.
                    foreach (var benchArea in _cardManager.enemyBenchAreas)
                    {
                        foreach (var benchSlot in benchArea.slots)
                        {
                            var occupiedPair = benchArea.GetFirstOccupiedSlotAndUnit();

                            if (occupiedPair.Value != null)
                            {
                                // 힐러인지 확인
                                if (occupiedPair.Value.TryGetComponent<FishUnit>(out var unit) && unit.CardData.Description.ToLower().Contains("healer"))
                                {
                                    continue; // 힐러는 건너뛰고 다음 슬롯을 확인
                                }

                                unitToMove = occupiedPair.Value;
                                sourceArea = benchArea;
                                sourceSlot = occupiedPair.Key;
                                break;
                            }
                        }

                        if (unitToMove != null) break;
                    }

                    if (unitToMove != null)
                    {
                        // 1. 기존 벤치 슬롯에서 유닛 해제
                        sourceArea.ReleaseSlot(sourceSlot);

                        // 2. 새로운 배틀 슬롯으로 이동
                        battleArea.OccupySlot(battleSlot, unitToMove);
                        unitToMove.transform.position = battleSlot.position;

                        // 3. 이동한 유닛의 부모 설정
                        unitToMove.transform.SetParent(battleSlot);
                        Debug.Log($"적 벤치의 유닛이 배틀 위치로 자동 이동했습니다.");

                        // CardDisplay의 위치 정보 업데이트 (옵션)
                        if (unitToMove.TryGetComponent<CardDisplay>(out var cardDisplay))
                        {
                            cardDisplay._currentSlotArea = battleArea;
                            cardDisplay._currentSlot = battleSlot;
                        }

                        return; // 한 슬롯만 교체하고 종료
                    }
                }
            }
        }
    }

    #endregion
}
