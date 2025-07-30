using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/FishSO")]
public class FishSO : ScriptableObject
{
    public int Id;
    public string FishName;
    public FishHabitatType HabitatType;
    public string FishDescription;
    public Sprite Icon;
    public int MaxStackSize;
    public float Weight = 1f;

    public override string ToString()
    {
        return $"[{Id}] {FishName} ({HabitatType}) - HP";
    }
}
