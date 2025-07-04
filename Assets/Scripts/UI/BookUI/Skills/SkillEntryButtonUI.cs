using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

using Models;

public class SkillEntryButtonUI : MonoBehaviour
{
    public string skillId;
    public Action<string> clickCallback;
    SkillEntry skillEntry;
    
    [SerializeField]Image image;

    //SFX
    [SerializeField] AudioClip ButtonPressSFX;

    void Start()
    {
        if (skillId != null)
        {
            skillEntry = SkillManager.Instance.GetEntry(skillId);

            if (skillEntry != null)
            {
                image.sprite = skillEntry.skill.skillIcon;
                SetButton();
            }
        }
    }

    public void SetButton()
    {
        if (skillEntry == null) return;

        if (skillEntry.active)
        {
            image.color = Color.green;
        }
        else if (skillEntry.available)
        {
            image.color = Color.white;
        }
        else
        {
            image.color = Color.gray;
        }
    }

    public void SetAsCurrent()
    {
        if (skillId != null && clickCallback != null) clickCallback(skillId);

        AudioSource.PlayClipAtPoint(ButtonPressSFX, Camera.main.transform.position, 1f);
    }
}
