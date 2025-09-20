using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI_Controller : MonoBehaviour
{
    private CardManager CardManager;
    public CardManager cardManager => CardManager;

    private EnemyBaseState _currentState;

    private void Start()
    {
        CardManager = FindObjectOfType<CardManager>();

        // 시작 상태를 Idle(대기) 상태로 설정합니다.
        ChangeState(new EnemyIdleState());
    }

    // 외부(TurnManager)에서 턴 시작을 알리는 메서드
    public void ExecuteTurn()
    {
        // 현재 상태의 로직을 실행합니다.
        // Idle 상태는 ExecuteTurn이 호출되면 PlayCards 상태로 전환하는 로직을 가집니다.
        StartCoroutine(_currentState.ExecuteState(this));
    }

    // 상태를 전환하는 핵심 메서드
    public void ChangeState(EnemyBaseState newState)
    {
        _currentState = newState;
        Debug.Log($"<color=cyan>적 AI 상태 전환: {_currentState.GetType().Name}</color>");

        // 새로운 상태가 즉시 로직을 시작해야 하는 경우
        // (예: 카드 사용이 끝나자마자 바로 공격 상태 로직 시작)
        // 아래 코드를 호출합니다.
        if (!(_currentState is EnemyIdleState)) // Idle 상태는 외부 호출을 기다림
        {
            StartCoroutine(_currentState.ExecuteState(this));
        }
    }
}
