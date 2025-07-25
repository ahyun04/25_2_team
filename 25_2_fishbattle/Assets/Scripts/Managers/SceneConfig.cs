using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SceneStatePair
{
    public string sceneName;
    public GameState gameState;
}

public class SceneConfig : SingletonMono<SceneConfig>
{
    #region 레퍼런스
    protected override bool DontDestroy => true;

    [Header("씬 상태 세팅")]
    public GameState defaultState = GameState.Menu;

    [Header("Scene Configurations")]
    public List<SceneStatePair> sceneConfigs = new List<SceneStatePair>();

    private Dictionary<string, GameState> _sceneStates = new Dictionary<string, GameState>();

    #endregion

    #region 초기화
    protected override void Awake()
    {
        base.Awake();
        SetupSceneStates();
        GameEvents.OnSceneChanged += OnSceneChanged;
    }

    private void OnDestroy()
    {
        GameEvents.OnSceneChanged -= OnSceneChanged;
    }

    #endregion

    #region 씬 전환
    private void SetupSceneStates()
    {
        _sceneStates.Clear();

        foreach (var config in sceneConfigs)
        {
            if (!string.IsNullOrEmpty(config.sceneName))
            {
                _sceneStates[config.sceneName] = config.gameState;
            }
        }
    }

    private void OnSceneChanged(string sceneName)
    {
        GameState targetState = _sceneStates.ContainsKey(sceneName) ? _sceneStates[sceneName] : defaultState;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeGameState(targetState);
        }

        Debug.Log($"씬 '{sceneName}' -> 상태: {targetState}");
    }

    // 런타임에서 씬 상태 변경
    public void SetSceneState(string sceneName, GameState state)
    {
        _sceneStates[sceneName] = state;
    }
    #endregion
}
