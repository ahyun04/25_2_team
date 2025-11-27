using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CollectionData
{
    public FishSO fish;
    public int count;
}

[System.Serializable]
public class CollectionSystem
{
    // Dictionary로 빠른 검색 + List로 직렬화(저장) 지원
    [SerializeField] private List<CollectionData> _collectedList = new List<CollectionData>();
    private Dictionary<FishSO, int> _collectionDict = new Dictionary<FishSO, int>();

    public void Initialize()
    {
        // 로드 시 리스트를 딕셔너리로 변환
        _collectionDict.Clear();
        foreach (var data in _collectedList)
        {
            if (!_collectionDict.ContainsKey(data.fish))
                _collectionDict.Add(data.fish, data.count);
        }
    }

    public bool HasFish(FishSO fish)
    {
        return _collectionDict.ContainsKey(fish);
    }

    public int GetFishCount(FishSO fish)
    {
        if (_collectionDict.TryGetValue(fish, out int count))
            return count;
        return 0;
    }

    public int GetCollectedTypeCount()
    {
        return _collectionDict.Count;
    }

    public bool RegisterFish(FishSO fish)
    {
        bool isNew = false;
        if (_collectionDict.ContainsKey(fish))
        {
            _collectionDict[fish]++;
            // 리스트 동기화 (저장용)
            var target = _collectedList.Find(x => x.fish == fish);
            if (target != null) target.count = _collectionDict[fish];
        }
        else
        {
            _collectionDict.Add(fish, 1);
            _collectedList.Add(new CollectionData { fish = fish, count = 1 });
            isNew = true;
        }

        return isNew; // 처음 잡았으면 true 반환
    }
}