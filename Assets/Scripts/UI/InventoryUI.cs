using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    private void Awake()
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


    [SerializeField] GameObject inventoryPanel;
    [SerializeField] GameObject inventoryItemTemplate;
    [SerializeField] Transform contentContainer;

    public void ToggleInventory()
    {
        // going from off to on -> update content first
        if(!inventoryPanel.activeSelf)
        {
            UpdateInventoryUI();
        }
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
    }

    public void UpdateInventoryUI()
    {
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        // get collected items
        Dictionary<string, InventoryItem> collectedItems = InventoryManager.Instance.GetCollectedItems();

        foreach (var entry in collectedItems)
        {
            InventoryItem item = entry.Value;

            // create new UI entry
            GameObject newItemUI = Instantiate(inventoryItemTemplate, contentContainer);
            newItemUI.SetActive(true);

            // populate entry with values
            newItemUI.transform.Find("Icon").GetComponent<Image>().sprite = item.itemData.itemIcon;
            newItemUI.transform.Find("Info/Title").GetComponent<TMP_Text>().text = $"{item.itemData.itemName} x{item.count}";
            //newItemUI.transform.Find("MaxSize").GetComponent<TMP_Text>().text = $"max: {item.largestSize}";
            newItemUI.transform.Find("Info/Details").GetComponent<TMP_Text>().text = GetRaritiesText(item.collectedRarities);
        }

        /*Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentContainer.GetComponent<RectTransform>());*/
    }

    private string GetRaritiesText(RarityType rarities)
    {
        List<string> rarityNames = new List<string>();

        if ((rarities & RarityType.Common) != 0) rarityNames.Add("Common");
        if ((rarities & RarityType.Rare) != 0) rarityNames.Add("Rare");
        if ((rarities & RarityType.Epic) != 0) rarityNames.Add("Epic");

        return string.Join(", ", rarityNames);
    }
}
