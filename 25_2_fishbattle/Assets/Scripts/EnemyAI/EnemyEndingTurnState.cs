using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEndingTurnState : EnemyBaseState
{
    public override IEnumerator ExecuteState(EnemyAI_Controller context)
    {
        yield return new WaitForSeconds(0.5f);

        TurnManager.Instance.EndTurn();

        // 턴이 끝나면 다시 Idle 상태로 돌아가 다음 턴을 기다립니다.
        context.ChangeState(new EnemyIdleState());
        yield break;
    }
}
