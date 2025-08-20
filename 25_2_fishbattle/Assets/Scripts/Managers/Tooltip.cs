using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Tooltip : MonoBehaviour
{
    #region 레퍼런스
    public TextMeshPro _nameText;
    [SerializeField] private TextMeshPro _hpText;
    [SerializeField] private TextMeshPro _skillNameText;
    [SerializeField] private TextMeshPro _abilityToActText;

    #endregion

    #region 셋업
    public void SetupTooltip(string name, int hp, string skillName, int act)
    {
        _nameText.text = name;
        _hpText.text = $"HP : {hp.ToString()}";
        _skillNameText.text = skillName;
        _abilityToActText.text = $"행동력 : {act.ToString()}";
    }

    #endregion
}
