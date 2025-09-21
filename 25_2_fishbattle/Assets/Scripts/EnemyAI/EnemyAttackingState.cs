using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyAttackingState : EnemyBaseState
{
    public override IEnumerator ExecuteState(EnemyAI_Controller context)
    {
        // 1. 공격 시작 시점에 공격자와 플레이어 유닛 목록을 딱 한 번만 가져옵니다.
        var enemyAttackers = context.cardManager.enemyBattleAreas.SelectMany(area => area.GetOccupiedCards()).ToList();

        // 2. 단일 루프로 각 공격 유닛의 공격을 순서대로 처리합니다.
        foreach (var attackerObj in enemyAttackers)
        {
            if (attackerObj == null || !attackerObj.TryGetComponent<FishUnit>(out var attackerUnit))
            {
                continue;
            }

            // 공격에 필요한 AP가 있는지 확인. (SpendAP는 AP가 있을 때만 true를 반환하고 소모시킴)
            if (!TurnManager.Instance.SpendAP(false, attackerUnit.CardData.AbilityToAct))
            {
                Debug.Log($"적 유닛 {attackerUnit.CardData.Name}은(는) AP가 부족하여 공격할 수 없습니다.");
                continue; // AP가 부족하면 다음 유닛으로 넘어감
            }

            // (공격 대상을 매번 새로 확인해야 합니다. 이전 공격으로 유닛이 죽었을 수 있기 때문입니다.)
            var livingPlayerUnits = context.cardManager.playerBattleAreas
                .SelectMany(area => area.GetOccupiedCards())
                .Where(p => p != null && p.GetComponent<FishUnit>()?.IsDead == false)
                .ToList();

            if (livingPlayerUnits.Count == 0)
            {
                Debug.Log("공격할 플레이어 유닛이 없습니다.");
                // 소모했던 AP를 다시 돌려줍니다.
                TurnManager.Instance.AddAP(false, attackerUnit.CardData.AbilityToAct);
                break; // 공격할 대상이 없으므로 공격 상태를 종료합니다.
            }

            // 간단하게 첫 번째 유닛을 공격 대상으로 지정
            GameObject targetObj = livingPlayerUnits[0];
            if (targetObj.TryGetComponent<FishUnit>(out var targetUnit))
            {
                Debug.Log($"적 유닛 {attackerUnit.CardData.Name}이(가) {targetUnit.CardData.Name}을(를) 공격! (소모 AP: {attackerUnit.CardData.AbilityToAct}, 남은 AP:{TurnManager.Instance.EnemyAP})");
                targetUnit.TakeDamage(attackerUnit.CardData.Damage);
            }

            // 공격 사이의 딜레이
            yield return new WaitForSeconds(0.35f);
        }

        // 3. 모든 유닛의 공격이 끝난 후, 다음 상태로 전환합니다.
        context.ChangeState(new EnemyReplacingUnitsState());
        yield break;
    }
}