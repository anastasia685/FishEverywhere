using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HiddenAreasManager : MonoBehaviour, IDataPersistence, ISkipPersistenceRegistration
{
    public static HiddenAreasManager Instance { get; private set; }
    Dictionary<string, Models.HiddenAreaEntry> entries = new();
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
        foreach (HiddenAreaSO hiddenArea in GameManager.Instance.GetHiddenAreas())
        {
            entries.Add(hiddenArea.id, new Models.HiddenAreaEntry(hiddenArea, false));
        }

        // update with data read from the disk
        foreach (var item in data.hiddenAreaEntries)
        {
            var currEntry = GetEntry(item.hiddenAreaId);
            if (currEntry == null) continue;

            currEntry.collected = item.collected;
        }
    }

    public void SaveData(GameData data)
    {
        data.hiddenAreaEntries.Clear();

        foreach (var entry in entries)
        {
            var serializableEntry = new Serializable.HiddenAreaEntry();
            serializableEntry.hiddenAreaId = entry.Key;
            serializableEntry.collected = entry.Value.collected;

            data.hiddenAreaEntries.Add(serializableEntry);
        }
    }

    public Models.HiddenAreaEntry GetEntry(string hiddenAreaId)
    {
        if (string.IsNullOrWhiteSpace(hiddenAreaId)) return null;

        return entries.TryGetValue(hiddenAreaId, out var entry) ? entry : null;
    }
    public Models.HiddenAreaEntry GetEntry(Vector3 position)
    {
        return entries.First(i => i.Value.hiddenArea.location == position).Value;
    }

    public void MarkEntryCollected(Vector3Int position)
    {
        var entry = GetEntry(position);
        if (entry == null) return;

        if (!entry.collected) AchievementManager.Instance.UpdateEntries(AchievementType.HiddenAreaCatch);

        entry.collected = true;
    }
}
