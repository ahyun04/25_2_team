using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NPCMoveState : NPCBaseState
{
    public override IEnumerator ExecuteState(NPC_AI_Controller context)
    {
        // 목적지 정하기
        Vector3 destination;
        if (context.GetRandomNavMeshPoint(context.centerPoint.position, context.wanderRadius, out destination))
        {
            // NavMeshAgent에게 목적지 설정 및 이동 시작
            context.Agent.isStopped = false;
            context.anim.SetBool("Walk", true);
            context.Agent.SetDestination(destination);

            // 목적지에 도착할 때까지 대기 (pathPending: 경로 계산 중), (remainingDistance: 남은 거리 > stoppingDistance: 멈추는 거리)
            while (context.Agent.pathPending || context.Agent.remainingDistance > context.Agent.stoppingDistance)
            {
                yield return null;
            }
        }
        else
        {
            // 랜덤 위치 찾기 실패
            Debug.LogWarning("NPC가 이동할 유효한 위치를 찾지 못했습니다.");
            yield return new WaitForSeconds(1.0f); // 1초 대기 후 다시 Idle로
        }

        // 도착했으므로 Idle 상태로 전환
        context.ChangeState(new NPCIdleState());
    }
}