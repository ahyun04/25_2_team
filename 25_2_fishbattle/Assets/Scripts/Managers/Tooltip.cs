using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Tooltip : MonoBehaviour
{
    #region 레퍼런스
    public TextMeshPro _nameText;
    [SerializeField] private TextMeshPro _hpText;
    [SerializeField] private TextMeshPro _descriptionText;
    [SerializeField] private TextMeshPro _abilityToActText;

    #endregion

    #region 셋업
    public void SetupTooltip(string name, int hp, string description, int act)
    {
        _nameText.text = name;
        _hpText.text = $"HP : {hp.ToString()}";
        _descriptionText.text = description;
        _abilityToActText.text = $"행동력 : {act.ToString()}";
    }

    #endregion
}
