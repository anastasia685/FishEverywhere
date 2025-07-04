using System;
using UnityEngine;

public class ActivateSkillButtonUI : MonoBehaviour
{
    //public SkillSO skillSO;
    [SerializeField] SkillPointsTextUI skillpointsTextScript;
    [SerializeField] SkillsPageUI pageUI;

    public string skillId;

    //SFX
    [SerializeField] AudioClip ButtonPressSFX;
    public void ActivateSkill()
    {
        //SFX
        AudioSource.PlayClipAtPoint(ButtonPressSFX, Camera.main.transform.position, 1f);

        SkillManager.Instance.ActivateSkill(skillId);
        skillpointsTextScript.RefreshText();

        // set buttons
        pageUI.SetButtons(skillId);

        //UI.SetButtons();
        //UI.SetBackground(skillSO);
    }
}
