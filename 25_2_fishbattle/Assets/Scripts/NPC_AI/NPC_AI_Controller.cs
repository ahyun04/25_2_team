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
    private Coroutine _lookAtCoroutine; // 바라보기 코루틴을 저장할 변수

    public bool IsPausedByDialogue { get; private set; } = false;

    [Header("설정")]
    public Transform centerPoint;
    public float wanderRadius = 10f;
    public float rotationSpeed = 5.0f; // 부드러운 회전을 위한 속도

    [Tooltip("이 NPC와 상호작용할 때 시작할 대화 ID")]
    public int startDialogId = 1;

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

    #region 상호작용 메서드
    public void PauseMovement()
    {
        IsPausedByDialogue = true;
        if (Agent != null && Agent.isOnNavMesh)
        {
            Agent.isStopped = true;
            Debug.Log("<color=yellow>NPC 이동 일시정지.</color>");
        }
    }

    public void ResumeMovement()
    {
        IsPausedByDialogue = false;
        if (Agent != null && Agent.isOnNavMesh)
        {
            Agent.isStopped = false;
            Debug.Log("<color=green>NPC 이동 재개.</color>");
        }
    }

    #endregion

    #region 대화 상호작용
    public void StartDialogueLook(Transform target)
    {
        if (_lookAtCoroutine != null)
        {
            StopCoroutine(_lookAtCoroutine);
        }

        _lookAtCoroutine = StartCoroutine(LookAtTargetRoutine(target));
    }

    public void EndDialogueLook()
    {
        if (_lookAtCoroutine != null)
        {
            StopCoroutine(_lookAtCoroutine);
            _lookAtCoroutine = null;
        }
    }

    private IEnumerator LookAtTargetRoutine(Transform target)
    {
        while (true)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }

            yield return null;
        }
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