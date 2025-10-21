using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Map : MonoBehaviour
{
    #region 레퍼런스
    [Header("맵 정보")]
    public MapType MapType;
    public string mapName = "Lake";

    [System.Serializable]
    public class MapEvents
    {
        public UnityEvent<MapType> OnMapEntered;
        public UnityEvent<string> OnMapExited;
    }

    public MapEvents mapEvents;

    #endregion

    #region 초기화
    void Start()
    {
        CreateNameTag();
    }

    #endregion

    #region 트리거
    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            mapEvents.OnMapEntered?.Invoke(MapType);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            mapEvents.OnMapExited?.Invoke(mapName);
            Debug.Log($"{mapName} 을 떠났습니다.");

            MapStateSystem mapStateSystem = FindObjectOfType<MapStateSystem>();
            if (mapStateSystem != null)
            {
                mapStateSystem.selectionPanel.SetActive(false);
            }
        }
    }

    #endregion

    #region 네임태그
    private void CreateNameTag()
    {
        // 건물 위에 이름표 생성
        GameObject nameTag = new GameObject("NameTag");
        nameTag.transform.SetParent(transform);
        nameTag.transform.localPosition = Vector3.up * 1.5f;

        TextMesh textMesh = nameTag.AddComponent<TextMesh>();
        textMesh.text = mapName;
        textMesh.characterSize = 0.2f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.color = Color.white;
        textMesh.fontSize = 20;

        nameTag.AddComponent<MapBoard>();
    }

    #endregion
}
