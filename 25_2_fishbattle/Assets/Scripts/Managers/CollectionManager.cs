using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectionManager : SingletonMono<CollectionManager>
{
    protected override bool DontDestroy => false;

    public bool IsFishCollected(FishSO fish)
    {
        if (CollectionHolder.Instance == null) return false;
        return CollectionHolder.Instance.CollectionSystem.HasFish(fish);
    }

    public int GetCollectedCount()
    {
        if (CollectionHolder.Instance == null) return 0;
        return CollectionHolder.Instance.CollectionSystem.GetCollectedTypeCount();
    }

    public void RegisterFishToCollection(FishSO fish)
    {
        if (CollectionHolder.Instance == null)
        {
            Debug.LogError("CollectionHolder가 없습니다!");
            return;
        }

        bool isSuccess = CollectionHolder.Instance.CollectionSystem.RegisterFish(fish);

        if (isSuccess)
        {
            Debug.Log($"[Manager] {fish.Name} 등록됨!");
        }
    }
}