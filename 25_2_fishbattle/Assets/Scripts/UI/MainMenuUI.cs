using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    #region 버튼 클래스
    [System.Serializable]
    private class SceneButton
    {
        public Button button;
        public string sceneName;
    }

    #endregion

    #region 레퍼런스
    [Header("씬 버튼")]
    [SerializeField] private List<SceneButton> m_sceneButtons;
    #endregion

    #region 씬 입력
    private void Start()
    {
        foreach (var sceneButton in m_sceneButtons)
        {
            if (sceneButton.button != null && !string.IsNullOrEmpty(sceneButton.sceneName))
            {
                sceneButton.button.onClick.AddListener(() => LoadScene(sceneButton.sceneName));
            }
        }
    }

    private void LoadScene(string sceneName)
    {
        SceneManager.Instance.LoadScene(sceneName);
    }

    #endregion
}
