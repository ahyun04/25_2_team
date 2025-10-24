using UnityEngine;

// 모든 미니게임의 실행 상태를 관리하는 정적 클래스.
public static class MiniGameManager
{
    // 현재 어떤 미니게임이든 실행 중인지 여부
    public static bool IsMiniGameRunning { get; private set; } = false;

    // 미니게임 시작을 시도합니다.
    // 시작에 성공하면 true, 다른 게임이 이미 실행 중이면 false
    public static bool TryStartMiniGame()
    {
        if (IsMiniGameRunning)
        {
            Debug.LogWarning("다른 미니게임이 이미 실행 중입니다. 시작할 수 없습니다.");
            return false;
        }

        IsMiniGameRunning = true;
        return true;
    }

    // 미니게임을 종료하고 잠금을 해제합니다.
    public static void EndMiniGame()
    {
        IsMiniGameRunning = false;
    }
}
