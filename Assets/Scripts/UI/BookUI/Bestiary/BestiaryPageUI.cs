using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BestiaryPageUI : MonoBehaviour, IBookPageUI
{
    public static BestiaryPageUI Instance;

    [SerializeField] public GameObject UI;
    [SerializeField] Transform OverviewListUI;


    [SerializeField] GameObject entryButtonPrefab;

    //buttons
    GameObject[] entryButtons;
    string initialEntryId;

    // page parts
    [SerializeField] Image image;
    [SerializeField] Text fish;
    [SerializeField] Text location;
    [SerializeField] Text description;
    [SerializeField] Text common, rare, epic, legendary, mythic;
    [SerializeField] Text min, max, largest;
    [SerializeField] Slider slider;

    //SFX
    [SerializeField] AudioClip BookOpenSFX;
    [SerializeField] AudioClip PageTurnSFX;

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

    public void Initialize()
    {
        if(entryButtons != null && entryButtons.Length > 0)
        {
            foreach (var button in entryButtons)
            {
                Destroy(button);
            }
        }
        entryButtons = BestiaryManager.Instance
                .Get()
                .Select((kvp, i) =>
                {
                    if (i == 0) initialEntryId = kvp.Key;

                    var button = Instantiate(entryButtonPrefab, OverviewListUI);
                    var buttonScript = button.GetComponent<BestiaryEntryButtonUI>();
                    buttonScript.catchableId = kvp.Key;
                    buttonScript.clickCallback = SetDetails;
                    return button;
                })
                .ToArray();
    }

    public void Show()
    {
        AudioSource.PlayClipAtPoint(BookOpenSFX, Camera.main.transform.position, 1f);

        SetPage();

        UI.SetActive(true);
    }

    public void Hide()
    {
        UI.SetActive(false);
    }

    public void SetAsActive()
    {
        BookUI.Instance.SetActivePage(Instance);
    }


    public void ToggleBeastiary()
    {
        // going from off to on -> update content first
        if (!UI.activeSelf)
        {
            //SFX
            AudioSource.PlayClipAtPoint(BookOpenSFX, Camera.main.transform.position, 1f);

            //SetPage(FishDetails);
            SetPage();

            //get rid of other pages
            SkillsPageUI.Instance.UI.SetActive(false);
            AchievementsPageUI.Instance.UI.SetActive(false);
        }
        UI.SetActive(!UI.activeSelf);
    }

    void SetDetails(string catchableId)
    {
        Models.BestiaryEntry entryData = BestiaryManager.Instance.GetEntry(catchableId);

        image.sprite = entryData.catchable.itemIcon;
        fish.text = entryData.catchable.itemName;
        location.text = entryData.catchable.itemLocation;
        description.text = entryData.catchable.itemDescription;
        common.text = entryData.caughtRarities[RarityType.Common].ToString();
        rare.text = entryData.caughtRarities[RarityType.Rare].ToString();
        epic.text = entryData.caughtRarities[RarityType.Epic].ToString();
        legendary.text = entryData.caughtRarities[RarityType.Legendary].ToString();
        mythic.text = entryData.caughtRarities[RarityType.Mythic].ToString();
        min.text = entryData.catchable.baseSize - entryData.catchable.sizeRange + "cm";
        max.text = entryData.catchable.baseSize + entryData.catchable.sizeRange + "cm";
        largest.text = entryData.largestSize + "cm";
        slider.minValue = entryData.catchable.baseSize - entryData.catchable.sizeRange;
        slider.maxValue = entryData.catchable.baseSize + entryData.catchable.sizeRange;
        slider.value = entryData.largestSize;

        if (entryData.caughtRarities.Values.Sum() == 0)
        {
            image.color = Color.black;
            description.text = "Catch a " + entryData.catchable.itemName + " to learn more.";
        }
        else
        {
            image.color = Color.white;
        }
    }

    void SetButtons()
    {
        foreach (var button in entryButtons)
        {
            var buttonScript = button.GetComponent<BestiaryEntryButtonUI>();
            buttonScript.SetButton();
        }
    }

    public void SetPage()
    {
        //SFX
        AudioSource.PlayClipAtPoint(PageTurnSFX, Camera.main.transform.position, 1f);

        SetDetails(initialEntryId);
        SetButtons();
    }

    public void UpdateBeastiary(BeastiaryEntrySO entry, RarityType rarity, int size)
    {
        if (size > entry.LargestCaught) 
        {
            entry.LargestCaught = size;
        }

        if (rarity == RarityType.Common)
        {
            entry.CommonCaught++;
        }
        else if (rarity == RarityType.Rare)
        {
            entry.RareCaught++;
        }
        else if (rarity == RarityType.Epic)
        {
            entry.EpicCaught++;
        }
    }
}
