using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AchievementManager : MonoBehaviour, IDataPersistence, ISkipPersistenceRegistration
{
    public static AchievementManager Instance { get; private set; }

    Dictionary<string, Models.AchievementEntry> entries = new();

    void Awake()
    {
        if(Instance == null)
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
        entries.Clear();


        // initialize entry list from scriptable objects
        foreach (AchievementSO achievement in GameManager.Instance.GetAchievements())
        {
            entries.Add(achievement.id, new Models.AchievementEntry(achievement, 0));
        }

        // update with data read from the disk
        foreach (var item in data.achievementEntries)
        {
            var currEntry = GetEntry(item.achievementId);
            if (currEntry == null) continue;

            currEntry.progress = item.progress;
        }
    }

    public void SaveData(GameData data)
    {
        data.achievementEntries.Clear();

        foreach (var entry in entries)
        {
            var serializableEntry = new Serializable.AchievementEntry();
            serializableEntry.achievementId = entry.Key;
            serializableEntry.progress = entry.Value.progress;

            data.achievementEntries.Add(serializableEntry);
        }
    }

    public Dictionary<string, Models.AchievementEntry> Get()
    {
        return entries;
    }
    public Models.AchievementEntry GetEntry(string catchableId)
    {
        if (string.IsNullOrWhiteSpace(catchableId)) return null;

        return entries.TryGetValue(catchableId, out var entry) ? entry : null;
    }
    public List<Models.AchievementEntry> GetEntries(AchievementType type)
    {
        return entries.Values.Where(i => i.achievement.type == type).ToList();
    }

    void UpdateEntry(string achievementId)
    {
        var entry = GetEntry(achievementId);
        if (entry == null) return;

        entry.progress++;
    }

    public void OnFishCaught(RarityType rarity, EnvironmentType environment)
    {
        UpdateEntries(AchievementType.Catch);
        UpdateEntries(AchievementType.RarityCatch, rarity);
        UpdateEntries(AchievementType.EnvironmentCatch, rarity, environment);
    }

    public void UpdateEntries(AchievementType type, RarityType rarity = RarityType.None, EnvironmentType environment = EnvironmentType.None)
    {
        var entriesToUpdate = GetEntries(type);
        switch (type)
        {
            case AchievementType.RarityCatch:
                var rarityEntries = entriesToUpdate
                    .Where(entry => entry.achievement is RarityAchievementSO && ((RarityAchievementSO)entry.achievement).rarity == rarity)
                    .ToList();
                DefaultUpdate(rarityEntries);
                break;
            case AchievementType.EnvironmentCatch:
                var environmentEntries = entriesToUpdate
                    .Where(entry => entry.achievement is EnvironmentAchievementSO && ((EnvironmentAchievementSO)entry.achievement).environment == environment)
                    .ToList();
                DefaultUpdate(environmentEntries);
                break;
            default:
                DefaultUpdate(entriesToUpdate);
                break;
        }
    }

    void DefaultUpdate(List<Models.AchievementEntry> entries)
    {
        foreach (var entry in entries)
        {
            entry.progress++;
        }
    }
}
