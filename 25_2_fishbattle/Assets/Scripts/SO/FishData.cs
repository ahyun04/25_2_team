using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FishData
{
    public int Id;
    public string FishName;
    public string HabitatTypeString;
    [NonSerialized] public FishHabitatType HabitatType;
    public string FishDescription;
    public Sprite Icon;
    public int MaxStackSize;

    // 문자열을 열거형으로 변환하는 메서드
    public void InitalizeEnums()
    {
        if (Enum.TryParse(HabitatTypeString, out FishHabitatType parsedType))
        {
            HabitatType = parsedType;
        }
        else
        {
            Debug.Log($"아이템 : '{FishName}'에 유효하지 않은 아이템 타입 : {HabitatTypeString}");
            // 기본값 설정
            HabitatType = FishHabitatType.Lake;
        }
    }
}
