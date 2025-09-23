using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyReplacingUnitsState : EnemyBaseState
{
    public override IEnumerator ExecuteState(EnemyAI_Controller context)
    {
        // [주석 추가] 벤치 유닛을 배틀 필드로 올릴지 말지 '고민'하는 시간을 표현하는 딜레이
        yield return new WaitForSeconds(1.0f);

        // 적 배틀 슬롯 배열을 순회합니다.
        foreach (var battleArea in context.cardManager.enemyBattleAreas)
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
                    foreach (var benchArea in context.cardManager.enemyBenchAreas)
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

                        // 유닛 이동이 끝난 후, 다음 행동(턴 종료)으로 넘어가기 전의 짧은 딜레이
                        yield return new WaitForSeconds(0.5f);

                        context.ChangeState(new EnemyEndingTurnState());
                        yield break;
                    }
                }
            }
        }

        Debug.Log("적 벤치 유닛 교체 로직 실행.");

        // 교체할 유닛이 없더라도, 턴을 종료하기 전 모든 행동이 완료되었음을 보여주는 최종 딜레이
        yield return new WaitForSeconds(0.5f);

        // 자신의 역할이 끝나면, 다음 상태로 전환
        context.ChangeState(new EnemyEndingTurnState());
        yield break; // 이 상태는 즉시 끝나므로 바로 종료
    }
}
