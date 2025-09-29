using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MapStateSystem : MonoBehaviour
{
    #region 레퍼런스
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
    }

    private void FindAllMap()
    {
        Map[] allMaps = FindObjectsOfType<Map>();

        foreach (Map map in allMaps)
        {
            if (map.FishHabitatType == FishHabitatType.Lake)
                _maps.Add(map);
            else if (map.FishHabitatType == FishHabitatType.River)
                _maps.Add(map);
            else if (map.FishHabitatType == FishHabitatType.Ocean)
                _maps.Add(map);
        }
    }

    #endregion

    #region 맵 선택
    public void OnPlayerEnteredMap(FishHabitatType mapType)
    {
        switch (mapType)
        {
            case FishHabitatType.Lake:
                SceneManager.Instance.LoadScene("LakeMiniGameScene");
                Debug.Log("호수 낚시터 씬으로 이동!");
                break;
            case FishHabitatType.River:
                SceneManager.Instance.LoadScene("RiverMiniGameScene");
                Debug.Log("강 낚시터 씬으로 이동!");
                break;
            case FishHabitatType.Ocean:
                SceneManager.Instance.LoadScene("OceanMiniGameScene");
                Debug.Log("바다 낚시터 씬으로 이동!");
                break;
            default:
                Debug.LogWarning($"'{mapType}'에 해당하는 씬이 없습니다.");
                break;
        }
    }

    #endregion 
}
