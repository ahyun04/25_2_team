using System.Collections;
using UnityEngine;

public class NPCIdleState : NPCBaseState
{
    // Idle 상태에서 얼마나 머무를지
    // 1초에서 3초 사이만 대기하도록 (값을 더 줄여도 됩니다)
    private float _minIdleTime = 1.0f;
    private float _maxIdleTime = 3.0f;

    public override IEnumerator ExecuteState(NPC_AI_Controller context)
    {
        // 멈추기
        if (context.Agent.isOnNavMesh)
        {
            context.Agent.isStopped = true;
        }

        // 랜덤 시간 동안 대기
        float idleTime = Random.Range(_minIdleTime, _maxIdleTime);
        yield return new WaitForSeconds(idleTime);

        // Move 상태로 전환
        context.ChangeState(new NPCMoveState());
    }
}