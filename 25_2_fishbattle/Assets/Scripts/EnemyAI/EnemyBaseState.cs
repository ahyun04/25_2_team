using System.Collections;
using UnityEngine;

// 모든 상태 클래스가 상속받을 추상 클래스
public abstract class EnemyBaseState
{
    public abstract IEnumerator ExecuteState(EnemyAI_Controller context);
}