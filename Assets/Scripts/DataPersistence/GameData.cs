using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public Vector3 position;

    // player stats
    public float rarityMultiplier;
    public float sizeMultiplier;
    public float timeScaleMultiplier;

    public int skillPoints;

    public List<Serializable.BestiaryEntry> bestiaryEntries;
    public List<Serializable.SkillEntry> skillEntries;
    public List<Serializable.AchievementEntry> achievementEntries;
    public List<Serializable.HiddenAreaEntry> hiddenAreaEntries;

    public GameData()
    {
        this.position = Vector3.zero;

        this.rarityMultiplier = 1.0f;
        this.sizeMultiplier = 1.0f;
        this.timeScaleMultiplier = 1.0f;

        this.skillPoints = 0;

        this.bestiaryEntries = new List<Serializable.BestiaryEntry>();
        this.skillEntries = new List<Serializable.SkillEntry>();
        this.achievementEntries = new List<Serializable.AchievementEntry>();
        this.hiddenAreaEntries = new List<Serializable.HiddenAreaEntry>();
    }
}
