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
    }

    #endregion
}
