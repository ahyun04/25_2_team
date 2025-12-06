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
            Debug.LogWarning($"[FishSpawner] {currentHabitat} (혹은 AllArea) 타입의 잡을 수 있는 플레이어 카드 물고기가 없습니다.");
            return null;
        }

        // 랜덤 선택 (확률(Probability) 가중치를 적용하려면 별도 로직 필요, 여기선 단순 랜덤)
        int randomIndex = Random.Range(0, availableFish.Count);
        return availableFish[randomIndex];
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

    // Lake, River, Ocean인 경우에만 AllArea 물고기를 포함시키기 위한 헬퍼
    private bool IsBasicHabitat(FishHabitatType type)
    {
        return type == FishHabitatType.Lake ||
               type == FishHabitatType.River ||
               type == FishHabitatType.Ocean;
    }
    #endregion
}