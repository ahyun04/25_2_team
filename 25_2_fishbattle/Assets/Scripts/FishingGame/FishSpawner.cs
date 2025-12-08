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
        FishHabitatType currentHabitat = GetHabitatFromScene();

        List<FishSO> candidateFishList = new List<FishSO>();

        List<FishSO> habitatFish = _fishDatabase.GetItemByType(currentHabitat);
        if (habitatFish != null)
            candidateFishList.AddRange(habitatFish);

        if (IsBasicHabitat(currentHabitat))
        {
            List<FishSO> allAreaFish = _fishDatabase.GetItemByType(FishHabitatType.AllArea);
            if (allAreaFish != null)
                candidateFishList.AddRange(allAreaFish);
        }

        List<FishSO> availableFish = candidateFishList
            .Where(fish => fish != null && fish.IsPlayerCard)
            .ToList();

        if (availableFish == null || availableFish.Count == 0)
        {
            Debug.LogWarning($"[FishSpawner] {currentHabitat} 타입의 잡을 수 있는 플레이어 카드 물고기가 없습니다.");
            return null;
        }

        float totalWeight = 0f;
        foreach (var fish in availableFish)
        {
            totalWeight += Mathf.Max(0, fish.Probability);
        }

        if (totalWeight <= 0f)
        {
            return availableFish[Random.Range(0, availableFish.Count)];
        }

        float randomValue = Random.Range(0f, totalWeight);

        foreach (var fish in availableFish)
        {
            float weight = Mathf.Max(0, fish.Probability);

            if (randomValue <= weight)
            {
                return fish;
            }

            randomValue -= weight;
        }

        return availableFish[availableFish.Count - 1];
    }

    private FishHabitatType GetHabitatFromScene()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        return sceneName switch
        {
            "LakeMiniGameScene" => FishHabitatType.Lake,
            "RiverMiniGameScene" => FishHabitatType.River,
            "OceanMiniGameScene" => FishHabitatType.Ocean,
            "DeepOceanMiniGameScene" => FishHabitatType.Abyss,
            _ => FishHabitatType.Lake
        };
    }

    private bool IsBasicHabitat(FishHabitatType type)
    {
        return type == FishHabitatType.Lake ||
               type == FishHabitatType.River ||
               type == FishHabitatType.Ocean;
    }
    #endregion
}