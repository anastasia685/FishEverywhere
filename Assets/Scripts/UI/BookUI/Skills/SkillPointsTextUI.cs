using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillPointsTextUI : MonoBehaviour
{
    Text skillPointsText;
    void Awake()
    {
        skillPointsText = gameObject.GetComponent<Text>();
    }
    void OnEnable()
    {
        RefreshText();
    }
    public void RefreshText()
    {
        if (skillPointsText != null) { skillPointsText.text = SkillManager.Instance.GetSkillPoints().ToString(); }
    }
}
