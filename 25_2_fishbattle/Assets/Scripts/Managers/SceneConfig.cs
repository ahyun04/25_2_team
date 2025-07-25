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
    #region ���۷���
    protected override bool DontDestroy => true;

    [Header("�� ���� ����")]
    public GameState defaultState = GameState.Menu;

    [Header("Scene Configurations")]
    public List<SceneStatePair> sceneConfigs = new List<SceneStatePair>();

    private Dictionary<string, GameState> _sceneStates = new Dictionary<string, GameState>();

    #endregion

    #region �ʱ�ȭ
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

    #region �� ��ȯ
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

        Debug.Log($"�� '{sceneName}' -> ����: {targetState}");
    }

    // ��Ÿ�ӿ��� �� ���� ����
    public void SetSceneState(string sceneName, GameState state)
    {
        _sceneStates[sceneName] = state;
    }
    #endregion
}
