using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPC_AI_Controller : MonoBehaviour
{
    #region 레퍼런스
    [Header("레퍼런스")]
    private NPCBaseState _currentState;
    public NavMeshAgent Agent { get; private set; }

    [Header("설정")]
    [Tooltip("NPC가 배회할 중심 지점입니다. (보통 자기 자신)")]
    public Transform centerPoint;
    [Tooltip("중심 지점에서 얼마나 멀리까지 배회할지 반경입니다.")]
    public float wanderRadius = 10f;

    #endregion

    #region 초기화
    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        if (centerPoint == null)
            centerPoint = this.transform; // 중심점이 없으면 자기 자신을 중심으로
    }

    private void Start()
    {
        ChangeState(new NPCIdleState());
    }

    #endregion

    #region 핵심 메서드
    public void ChangeState(NPCBaseState newState)
    {
        _currentState = newState;
        Debug.Log($"<color=white>NPC AI 상태 전환: {_currentState.GetType().Name}</color>");

        StartCoroutine(_currentState.ExecuteState(this));
    }

    #endregion

    #region 유틸리티 메서드
    /// <summary>
    /// 지정된 반경 내의 NavMesh 위 랜덤한 지점을 찾아 반환합니다.
    /// </summary>
    /// <param name="origin">중심점</param>
    /// <param name="dist">반경</param>
    /// <param name="result">찾은 위치</param>
    /// <returns>성공 여부</returns>
    public bool GetRandomNavMeshPoint(Vector3 origin, float dist, out Vector3 result)
    {
        for (int i = 0; i < 30; i++) // 30번 시도
        {
            Vector3 randomDirection = Random.insideUnitSphere * dist;
            randomDirection += origin;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, 1.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = Vector3.zero;
        return false;
    }

    #endregion
}