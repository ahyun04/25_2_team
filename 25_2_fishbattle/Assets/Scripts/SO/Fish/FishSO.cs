using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/FishSO")]
public class FishSO : ScriptableObject
{
    public int FishId;
    public string Name;
    public string Description;
    public Position Position;
    public string Skill_name;
    public int Damage;
    public int Heal;
    public int Hp;
    public int AbilityToAct;
    public FishHabitatType HabitatType;
    public float Probability;
    public int MaxStackSize;
    public bool IsPlayerCard = false;
    public Sprite Icon;

    [Header("물고기 프리팹")]
    [SerializeField] private GameObject _prefab;

    public GameObject Prefab
    {
        get => _prefab;
        set => _prefab = value;
    }

    public FishSO CreateEnhancedInstance()
    {
        // 1. 현재 SO를 복제 (Instantiate)
        FishSO newFish = Instantiate(this);

        // 2. 이름 변경 (선택 사항)
        newFish.Name = $"[강화된] {this.Name}";
        newFish.name = newFish.Name; // 내부 에셋 이름 변경

        // 3. 능력치 강화 로직 적용
        // 체력 1.5배 (int 캐스팅으로 소수점 버림)
        newFish.Hp = (int)(this.Hp * 1.5f);

        // 공격력 2배
        newFish.Damage = this.Damage * 2;

        // 행동력 소모 1 추가
        newFish.AbilityToAct += 1;

        return newFish;
    }
}
