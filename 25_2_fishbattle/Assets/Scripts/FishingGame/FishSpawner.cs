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
        List<FishSO> fishListByHabitat = _fishDatabase.GetItemByType(habitat);
        List<FishSO> availableFish = fishListByHabitat.Where(fish => fish.IsPlayerCard).ToList();

        if (availableFish == null || availableFish.Count == 0)
        {
            Debug.LogWarning($"[FishSpawner] {habitat} 타입의 잡을 수 있는 플레이어 카드 물고기가 없습니다.");
            return null;
        }

        // 3. 필터링된 리스트를 기반으로 가중치 랜덤 로직을 실행합니다.
        float totalWeight = availableFish.Sum(fish => fish.Weight);
        float roll = Random.Range(0f, totalWeight);
        float accumulator = 0f;

        foreach (var fish in availableFish)
        {
            accumulator += fish.Weight;
            if (roll <= accumulator)
                return fish;
        }

        return availableFish.FirstOrDefault(); // fallback
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