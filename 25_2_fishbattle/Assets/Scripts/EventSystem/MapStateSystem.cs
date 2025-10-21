using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class MapStateSystem : MonoBehaviour
{
    #region 레퍼런스
    [Header("UI 레퍼런스")]
    public GameObject selectionPanel;
    public Button selectionButton;
    public Button gameButton;

    private List<Map> _maps = new List<Map>();

    // 필요한 Event 시스템 있으면 넣기
    [System.Serializable]
    public class MapStateEvents { }

    public MapStateEvents mapStateEvents;

    #endregion

    #region 초기화
    void Start()
    {
        FindAllMap();

        selectionButton.onClick.AddListener(LoadOceanScene);
        gameButton.onClick.AddListener(LoadCardGameScene);
    }

    private void FindAllMap()
    {
        Map[] allMaps = FindObjectsOfType<Map>();

        foreach (Map map in allMaps)
        {
            if (map.MapType == MapType.Lake)
                _maps.Add(map);
            else if (map.MapType == MapType.River)
                _maps.Add(map);
            else if (map.MapType == MapType.Ocean)
                _maps.Add(map);
        }
    }

    #endregion

    #region 맵 선택
    public void OnPlayerEnteredMap(MapType mapType)
    {
        switch (mapType)
        {
            case MapType.Lake:
                SceneManager.Instance.LoadScene("LakeMiniGameScene");
                Debug.Log("호수 낚시터 씬으로 이동!");
                break;
            case MapType.River:
                SceneManager.Instance.LoadScene("RiverMiniGameScene");
                Debug.Log("강 낚시터 씬으로 이동!");
                break;
            case MapType.Ocean:
                if (selectionPanel != null)
                {
                    selectionPanel.SetActive(true);
                    Debug.Log("바다 맵 진입! 선택 패널을 엽니다.");
                }
                else
                {
                    Debug.LogWarning("Selection Panel이 연결되지 않았습니다!");
                }
                break;
            case MapType.Enhancement:
                SceneManager.Instance.LoadScene("EnhancementScene");
                Debug.Log("강 낚시터 씬으로 이동!");
                break;
            default:
                Debug.LogWarning($"'{mapType}'에 해당하는 씬이 없습니다.");
                break;
        }
    }

    #endregion

    #region UI 버튼 연결용 메서드
    public void LoadOceanScene()
    {
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(false);
        }
        SceneManager.Instance.LoadScene("OceanMiniGameScene");
    }

    public void LoadCardGameScene()
    {
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(false);
        }
        SceneManager.Instance.LoadScene("FishCardGameScene");
    }

    #endregion
}
