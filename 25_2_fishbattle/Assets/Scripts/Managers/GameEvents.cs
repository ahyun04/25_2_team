// ���� ��ü���� ����� �̺�Ʈ ����
public static class GameEvents
{
    public static System.Action<string> OnSceneChanged;
    public static System.Action OnGamePaused;
    public static System.Action OnGameResumed;

    // �̺�Ʈ ȣ�� �޼����
    public static void SceneChanged(string sceneName) => OnSceneChanged?.Invoke(sceneName);
    public static void GamePaused() => OnGamePaused?.Invoke();
    public static void GameResumed() => OnGameResumed?.Invoke();
}