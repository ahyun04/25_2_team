using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class QuitButton : MonoBehaviour
{
    void Start()
    {
        Button btn = GetComponent<Button>();

        // 기존에 Inspector에 연결된 리스너가 있다면 모두 제거
        btn.onClick.RemoveAllListeners();

        // 현재 살아있는 GameManager.Instance의 QuitGame 함수를 리스너로 등록
        if (GameManager.Instance != null)
        {
            btn.onClick.AddListener(GameManager.Instance.QuitGame);
        }
        else
        {
            Debug.LogError("QuitButton이 GameManager.Instance를 찾을 수 없습니다!", gameObject);
        }
    }
}
