using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/FishSO")]
public class FishSO : ScriptableObject
{
    public int FishId;
    public string Name;
    public int Hp;
    public int AbilityToAct;
    public Sprite Icon;
    public string Skill_name;
    public int Damage;
    public int Heal;
    public int Support;
    public FishHabitatType HabitatType;
    public int Probability;
    public string Description;
    public int MaxStackSize;
    public float Weight = 1f;
    public bool IsPlayerCard = false;
    public bool IsCheck = false;

    [Header("물고기 프리팹")]
    [SerializeField] private GameObject _prefab;
    public GameObject Prefab
    {
        get => _prefab;
        set => _prefab = value;
    }
}
