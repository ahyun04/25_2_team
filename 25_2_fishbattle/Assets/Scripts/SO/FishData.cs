using System;

[Serializable]
public class FishData
{
    public int FishId;
    public string Name;
    public int Hp;
    public int AbilityToAct;
    public string AbilityToAct_icon;
    public string Skill_name;
    public int Damage;
    public int Heal;
    public int Support;
    public string HabitatTypeString;
    [NonSerialized] public FishHabitatType HabitatType;
    public int Probability;
    public string Description;
    public int MaxStackSize;
    public float Weight = 1f;
    public bool IsPlayerCard = false;
    public bool IsCheck = false;
    public string Prefab;

    public void InitalizeEnums()
    {
        if (Enum.TryParse(HabitatTypeString, out FishHabitatType parsedType))
            HabitatType = parsedType;
        else
            HabitatType = FishHabitatType.Lake;
    }
}
