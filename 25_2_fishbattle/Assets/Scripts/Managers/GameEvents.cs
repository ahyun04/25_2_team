// 게임 전체에서 사용할 이벤트 정의
public static class GameEvents
{
    public static System.Action<string> OnSceneChanged;
    public static System.Action OnGamePaused;
    public static System.Action OnGameResumed;

    // 이벤트 호출 메서드들
    public static void SceneChanged(string sceneName) => OnSceneChanged?.Invoke(sceneName);
    public static void GamePaused() => OnGamePaused?.Invoke();
    public static void GameResumed() => OnGameResumed?.Invoke();
}