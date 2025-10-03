public class EnhancementHolder : SingletonMono<EnhancementHolder>
{
    protected override bool DontDestroy => true;

    public EnhancementSystem EnhancementSystem { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        EnhancementSystem = new EnhancementSystem();
    }
}