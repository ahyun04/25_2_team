using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialog", menuName = "Dialog/DialogSO")]
public class DialogSO : ScriptableObject
{
    public int DialogId;
    public string name;
    public string text;
    public List<DialogChoiceSO> choices =new List<DialogChoiceSO>();
    [Header("Npc ÇÁ¸®ÆÕ")]
    [SerializeField] private GameObject _portraitPath;

    public GameObject PortraitPath
    {
        get => _portraitPath;
        set => _portraitPath = value;
    }
}
