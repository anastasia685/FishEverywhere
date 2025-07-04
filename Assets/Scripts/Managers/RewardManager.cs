using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting;
using UnityEngine;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance { get; private set; }

    private CatchableSO pendingItem;
    private EnvironmentType pendingEnvironment;
    private RarityType pendingRarity;
    private int pendingSize;
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

    public void ChooseReward(EnvironmentType environment)
    {
        List<CatchableSO> matchingItems = GameManager.Instance.GetCatchables(environment);

        if (matchingItems.Count == 0)
        {
            Debug.Log("No catchables found for this environment");
            pendingItem = null;
            return;
        }

        List<int> weights = matchingItems.Select(item => item.environmentCounts[item.environments.IndexOf(environment)]).ToList();
        int totalWeight = weights.Sum();

        int rand = Random.Range(0, totalWeight);

        int weightCount = 0;
        for(int i = 0; i < matchingItems.Count; i++)
        {
            weightCount += weights[i];
            if(rand < weightCount)
            {
                pendingItem = matchingItems[i];
                break;
            }
        }

        //pendingItem = matchingItems[Random.Range(0, matchingItems.Count)];
        pendingEnvironment = environment;
        pendingRarity = ChooseRarity();
        pendingSize = ChooseSize(pendingItem.baseSize, pendingItem.sizeRange);
    }

    static RarityType ChooseRarity()
    {
        int rand = Random.Range(0, 100);

        rand = (int)(rand * PlayerStatsManager.Instance.GetRarityMultiplier());

        if (rand > 115) return RarityType.Mythic;
        else if (rand > 100) return RarityType.Legendary;
        else if (rand > 85) return RarityType.Epic;
        else if (rand > 60) return RarityType.Rare;
        else return RarityType.Common;
    }

    static int ChooseSize(int baseSize, int sizeRange)
    {
        int sizeIncrement = Random.Range(-sizeRange, sizeRange + 1);
        //int sizeIncrement = Random.Range(0, sizeRange + 1);
        //sizeIncrement = (int)(sizeIncrement * PlayerStats.SizeMultiplier);
        
        return baseSize + sizeIncrement;
    }

    public CatchableSO GetPendingItem() => pendingItem;
    public EnvironmentType GetPendingEnvironment() => pendingEnvironment;
    public RarityType GetPendingRarity() => pendingRarity;
    public int GetPendingSkillPoints() 
    {
        switch(pendingRarity)
        {   
            case RarityType.Mythic:
                return 50;
            case RarityType.Legendary:
                return 20;
            case RarityType.Epic:
                return 10;    
            case RarityType.Rare:
                return 5;
            case RarityType.Common:
            default:
                return 2;
        }
    }
    public int GetPendingSize() => pendingSize;

    public void ClearPendingReward()
    {
        pendingItem = null;
    }
}

