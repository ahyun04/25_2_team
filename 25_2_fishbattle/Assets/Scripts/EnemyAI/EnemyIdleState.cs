using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIdleState : EnemyBaseState
{
    public override IEnumerator ExecuteState(EnemyAI_Controller context)
    {
        context.ChangeState(new EnemyPlayingCardsState());
        yield break;
    }
}
