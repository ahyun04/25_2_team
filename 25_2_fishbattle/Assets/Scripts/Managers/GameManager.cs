using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonMono<GameManager>
{
    #region ���۷���
    protected override bool DontDestroy => true;

    #endregion

    #region �ʱ�ȭ
    protected override void Awake()
    {
        base.Awake();
    }

    #endregion
}
