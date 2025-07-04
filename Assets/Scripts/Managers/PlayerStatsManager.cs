using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatsManager : MonoBehaviour, IDataPersistence, ISkipPersistenceRegistration
{
    public static PlayerStatsManager Instance { get; private set; }

    float rarityMultiplier = 1.0f;
    float sizeMultiplier = 1.0f;
    float timeScaleMultiplier = 1.0f;

    public float GetRarityMultiplier() { return rarityMultiplier; }
    public float GetSizeMultiplier() { return sizeMultiplier; }
    public float GetTimeScaleMultiplier() { return timeScaleMultiplier; }


    public void LoadData(GameData data)
    {
        this.rarityMultiplier = data.rarityMultiplier;
        this.sizeMultiplier = data.sizeMultiplier;
        this.timeScaleMultiplier = data.timeScaleMultiplier;
    }

    public void SaveData(GameData data)
    {
        data.rarityMultiplier = this.rarityMultiplier;
        data.sizeMultiplier = this.sizeMultiplier;
        data.timeScaleMultiplier = this.timeScaleMultiplier;
    }



    public void UpdateRarityMultiplier(float val)
    {
        rarityMultiplier = Mathf.Max(rarityMultiplier, val);
    }
    public void UpdateSizeMultiplier(float val)
    {
        sizeMultiplier = Mathf.Max(sizeMultiplier, val);
    }
    public void UpdateTimeScaleMultiplier(float val)
    {
        timeScaleMultiplier = Mathf.Max(timeScaleMultiplier, val);
    }

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
}
