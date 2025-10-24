using UnityEngine;

public class ESCMenuUI : MonoBehaviour
{
    public GameObject ESC_Panel;

    void Start()
    {
        if (ESC_Panel != null)
        {
            ESC_Panel.SetActive(false);
        }

        // GameManager가 보내는 일시정지/재개 이벤트를 구독합니다.
        GameEvents.OnGamePaused += ShowPausePanel;
        GameEvents.OnGameResumed += HidePausePanel;
    }

    private void OnDestroy()
    {
        // 씬이 파괴될 때 이벤트 구독을 해제합니다.
        GameEvents.OnGamePaused -= ShowPausePanel;
        GameEvents.OnGameResumed -= HidePausePanel;
    }

    // 게임이 일시정지되면(알트탭 포함) 패널을 켭니다.
    private void ShowPausePanel()
    {
        if (ESC_Panel != null)
        {
            ESC_Panel.SetActive(true);
        }
    }

    private void HidePausePanel()
    {
        if (ESC_Panel != null)
        {
            ESC_Panel.SetActive(false);
        }
    }
}