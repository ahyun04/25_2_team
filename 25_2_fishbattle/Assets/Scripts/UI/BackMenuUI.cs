using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackMenuUI : MonoBehaviour
{
    #region 레퍼런스
    [Header("레퍼런스")]
    [SerializeField] private Button _loadMainMenuButton;

    #endregion

    #region 초기화
    private void Start()
    {
        SetupButtons();

        // 이벤트 구독
        GameEvents.OnSceneChanged += OnSceneChanged;
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        GameEvents.OnSceneChanged -= OnSceneChanged;
    }

    #endregion

    #region 버튼 세팅
    private void SetupButtons()
    {
        // 씬 로딩 버튼
        if (_loadMainMenuButton != null)
            _loadMainMenuButton.onClick.AddListener(() => SceneManager.Instance.LoadMainMenu());
    }

    private void OnSceneChanged(string sceneName)
    {
        Debug.Log($"씬 변경 이벤트 수신: {sceneName}");
    }

    #endregion
}
