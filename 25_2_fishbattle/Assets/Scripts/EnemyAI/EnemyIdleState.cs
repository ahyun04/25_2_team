using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIdleState : EnemyBaseState
{
    public override IEnumerator ExecuteState(EnemyAI_Controller context)
    {
        while (TurnManager.Instance.CurrentTurn != TurnManager.TeamTurn.Enemy)
        {
            yield return null;
        }

        context.ChangeState(new EnemyPlayingCardsState());
    }
}
