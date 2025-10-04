using UnityEngine;

public class FishingHolder : SingletonMono<FishingHolder>
{
    protected override bool DontDestroy => true;

    public FishingSystem FishingSystem { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        FishingSystem = new FishingSystem();
    }

    private void Update()
    {
        FishingSystem.UpdateTimers(Time.deltaTime);
    }
}