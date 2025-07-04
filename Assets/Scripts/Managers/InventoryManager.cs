using System.Collections.Generic;
using UnityEngine;


// in case i want to add more metadata
public class InventoryItem
{
    public CatchableSO itemData;
    public int count;
    public int largestSize;
    public RarityType collectedRarities;
    public InventoryItem(CatchableSO item, RarityType rarity, int size)
    {
        itemData = item;
        count = 1;
        collectedRarities = rarity;
        largestSize = size;
    }

    public void UpdateCatch(RarityType rarity, int size)
    {
        count++;
        collectedRarities |= rarity;
        largestSize = Mathf.Max(largestSize, size);
    }
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    private Dictionary<string, InventoryItem> collectedItems = new Dictionary<string, InventoryItem>();
    public Dictionary<string, InventoryItem> GetCollectedItems()
    {
        return collectedItems;
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

    public void AddItem(CatchableSO item, RarityType rarity, int size)
    {
        if (collectedItems.ContainsKey(item.itemName))
        {
            collectedItems[item.itemName].UpdateCatch(rarity, size);
        }
        else
        {
            collectedItems[item.itemName] = new InventoryItem(item, rarity, size);
        }

        //Debug.Log($"Collected {item.title}: Count = {collectedItems[item.title].count}");
    }

}
