using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialog", menuName = "Dialog/DialogChoiceSO")]
public class DialogChoiceSO : ScriptableObject
{
    public int choiceId;
    public string choiceText;
    public int choiceNextId;
}
