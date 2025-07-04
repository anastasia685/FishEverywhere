using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BestiaryManager : MonoBehaviour, IDataPersistence, ISkipPersistenceRegistration
{
    public static BestiaryManager Instance { get; private set; }
    Dictionary<string, Models.BestiaryEntry> entries = new();

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
        entries.Clear();


        // initialize entry list from scriptable objects
        foreach (CatchableSO catchable in GameManager.Instance.GetCatchables())
        {
            entries.Add(catchable.id, new Models.BestiaryEntry(catchable, 0, new Dictionary<RarityType, int>(Models.BestiaryEntry.InitialRarities)));
        }


        // update with data read from the disk
        foreach (var item in data.bestiaryEntries)
        {
            var currEntry = GetEntry(item.catchableId);
            if (currEntry == null) continue;

            currEntry.largestSize = item.largestSize;
            foreach (var pair in item.caughtRarities)
            {
                if (!currEntry.caughtRarities.ContainsKey(pair.Key)) continue;

                currEntry.caughtRarities[pair.Key] = item.caughtRarities[pair.Key];
            }
            //entries.Add(item.catchableId, new BestiaryEntry(GameManager.Instance.GetCatchable(item.catchableId), item.largestSize, item.caughtRarities));
        }
    }

    public void SaveData(GameData data)
    {
        data.bestiaryEntries.Clear();

        foreach (var entry in entries)
        {
            var serializableEntry = new Serializable.BestiaryEntry();
            serializableEntry.catchableId = entry.Key;
            serializableEntry.largestSize = entry.Value.largestSize;
            serializableEntry.caughtRarities = new Serializable.Dictionary<RarityType, int>();
            foreach (var pair in entry.Value.caughtRarities)
            {
                serializableEntry.caughtRarities.Add(pair.Key, pair.Value);
            }

            data.bestiaryEntries.Add(serializableEntry);
        }
    }

    public Dictionary<string, Models.BestiaryEntry> Get()
    {
        return entries;
    }
    public Models.BestiaryEntry GetEntry(string catchableId)
    {
        if (string.IsNullOrWhiteSpace(catchableId)) return null;

        return entries.TryGetValue(catchableId, out var entry) ? entry : null;
    }

    public void UpdateEntry(string catchableId, RarityType rarity, int size)
    {
        var entry = GetEntry(catchableId);
        if (entry == null) return;

        if(size == entry.catchable.baseSize + entry.catchable.sizeRange)
        {
            AchievementManager.Instance.UpdateEntries(AchievementType.MaxSizeCatch);
        }
        if (entry.caughtRarities[rarity] == 0)
        {
            AchievementManager.Instance.UpdateEntries(AchievementType.BestiaryEntry);
        }

        entry.largestSize = Mathf.Max(entry.largestSize, size);
        entry.caughtRarities[rarity]++;
    }
}
