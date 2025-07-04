using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillManager : MonoBehaviour, IDataPersistence, ISkipPersistenceRegistration
{
    public static SkillManager Instance { get; private set; }

    int skillPoints = 0;
    [SerializeField] public SkillSO rootSO;

    private Dictionary<string, Models.SkillEntry> entries = new();

    public Dictionary<string, Models.SkillEntry> Get()
    {
        return entries;
    }

    //SFX
    [SerializeField] AudioClip UnlockSkillSFX;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadData(GameData data)
    {
        this.skillPoints = data.skillPoints;


        entries.Clear();

        // initialize entry list from scriptable objects
        TraverseAndRegister(rootSO);

        // update with data read from the disk
        foreach (var item in data.skillEntries)
        {
            var currEntry = entries[item.skillId];
            if (currEntry == null) continue;

            currEntry.available = item.available;
            currEntry.active = item.active;
        }
    }

    public void SaveData(GameData data)
    {
        data.skillPoints = this.skillPoints;

        data.skillEntries.Clear();
        foreach (var entry in entries)
        {
            var serializableEntry = new Serializable.SkillEntry();
            serializableEntry.skillId = entry.Key;
            serializableEntry.available = entry.Value.available;
            serializableEntry.active = entry.Value.active;

            data.skillEntries.Add(serializableEntry);
        }
    }

    void TraverseAndRegister(SkillSO skill)
    {
        if (skill == null || string.IsNullOrWhiteSpace(skill.id)) return;

        if (!entries.ContainsKey(skill.id))
            entries.Add(skill.id, new Models.SkillEntry(skill, skill.id == rootSO.id, false));

        foreach (var child in skill.children)
        {
            TraverseAndRegister(child);
        }
    }

    public Models.SkillEntry GetEntry(string skillId)
    {
        if(string.IsNullOrWhiteSpace(skillId)) return null;

        return entries.TryGetValue(skillId, out var entry) ? entry : null;
    }

    public void ActivateSkill(string skillId)
    {
        var entry = GetEntry(skillId);

        if (entry == null || entry.active || !entry.available || skillPoints < entry.skill.cost) return;

        entry.active = true;
        UpdatePlayerStats(entry.skill.buffType, entry.skill.buffMultiplier);

        foreach(SkillSO child in entry.skill.children)
        {
            if(child != null)
            {
                var childEntry = GetEntry(child.id);
                if (childEntry != null) childEntry.available = true;
            }
        }
        skillPoints -= entry.skill.cost;

        //SFX
        AudioSource.PlayClipAtPoint(UnlockSkillSFX, Camera.main.transform.position, 1f);

        //update achivement
        //AchievementsPageUI.Instance.SkillsUpdate();
        AchievementManager.Instance.UpdateEntries(AchievementType.GainSkill);
    }

    void UpdatePlayerStats(BuffType buffType, float buffMultiplier)
    {
        switch(buffType)
        {
            case BuffType.Rarity:
                PlayerStatsManager.Instance.UpdateRarityMultiplier(buffMultiplier);
                break;
            case BuffType.Size:
                PlayerStatsManager.Instance.UpdateSizeMultiplier(buffMultiplier);
                break;
            case BuffType.TimeScale:
                PlayerStatsManager.Instance.UpdateTimeScaleMultiplier(buffMultiplier);
                break;
        }
    }

    public void IncrementPoints(int points)
    {
        skillPoints += points;
    }
    public int GetSkillPoints() => skillPoints;
}
