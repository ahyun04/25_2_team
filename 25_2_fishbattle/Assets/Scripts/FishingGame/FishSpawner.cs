using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    #region 레퍼런스
    [SerializeField] private FishDatabaseSO _fishDatabase;

    #endregion

    #region 물고기 스폰 관련
    public FishSO GetRandomFishByScene()
    {
        FishHabitatType habitat = GetHabitatFromScene();
        List<FishSO> fishList = _fishDatabase.GetItemByType(habitat);

        if (fishList == null || fishList.Count == 0)
        {
            Debug.LogWarning($"[FishSpawner] {habitat} 타입 물고기가 없습니다.");
            return null;
        }

        float totalWeight = fishList.Sum(fish => fish.Weight);
        float roll = Random.Range(0f, totalWeight);
        float accumulator = 0f;

        foreach (var fish in fishList)
        {
            accumulator += fish.Weight;
            if (roll <= accumulator)
                return fish;
        }

        return fishList.FirstOrDefault(); // fallback
    }

    private FishHabitatType GetHabitatFromScene()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        return sceneName switch
        {
            "LakeMiniGameScene" => FishHabitatType.Lake,
            "RiverMiniGameScene" => FishHabitatType.River,
            "OceanMiniGameScene" => FishHabitatType.Ocean,
            _ => FishHabitatType.Lake
        };
    }

    #endregion
}
