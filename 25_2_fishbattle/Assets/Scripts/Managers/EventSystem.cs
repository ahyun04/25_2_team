using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSystem : SingletonMono<EventSystem>
{
    #region 레퍼런스
    public bool isEventPlaying = false;

    #endregion

    #region 이벤트
    // 이벤트 시작
    public void StartEvent()
    {
        if (isEventPlaying) return;

        isEventPlaying = true;
        GameManager.Instance?.PauseGame();

        Debug.Log("이벤트 시작 - 게임 일시정지");
    }

    // 이벤트 종료
    public void EndEvent()
    {
        if (!isEventPlaying) return;

        isEventPlaying = false;
        GameManager.Instance?.ResumeGame();

        Debug.Log("이벤트 종료 - 게임 재개");
    }

    // 이벤트 연출 예제
    public void PlayCutscene(float duration)
    {
        StartCoroutine(CutsceneCoroutine(duration));
    }

    private IEnumerator CutsceneCoroutine(float duration)
    {
        StartEvent();

        Debug.Log($"{duration}초 컷신 재생");
        yield return new WaitForSecondsRealtime(duration);

        EndEvent();
    }

    #endregion
}
