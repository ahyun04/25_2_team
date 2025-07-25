using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    #region ��ư Ŭ����
    [System.Serializable]
    private class SceneButton
    {
        public Button button;
        public string sceneName;
    }

    #endregion

    #region ���۷���
    [Header("�� ��ư")]
    [SerializeField] private List<SceneButton> m_sceneButtons;
    #endregion

    #region �� �Է�
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
