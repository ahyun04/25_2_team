using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonMono<GameManager>
{
    #region 레퍼런스
    protected override bool DontDestroy => true;

    #endregion

    #region 초기화
    protected override void Awake()
    {
        base.Awake();
        InitializeGame();
    }

    // 게임 기본 설정
    private void InitializeGame()
    {
        Application.targetFrameRate = 60;

        // 더 생기면 추가 가능
    }

    #endregion
}
