using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class AchievementsPageUI : MonoBehaviour, IBookPageUI
{
    public static AchievementsPageUI Instance;

    [SerializeField] public GameObject UI;
    [SerializeField] Transform OverviewListUI;
    string initialEntryId;
    //public AchievementSO root;

    [SerializeField] GameObject entryButtonPrefab;

    //buttons
    //public Button[] Buttons;
    GameObject[] entryButtons;

    // page parts
    [SerializeField] Image image;
    [SerializeField] Text achivementTitle;
    [SerializeField] Text description;
    [SerializeField] Text BronzeBar, SilverBar, GoldBar;
    [SerializeField] Text progress;

    //SFX
    [SerializeField] AudioClip BookOpenSFX;
    [SerializeField] AudioClip PageTurnSFX;
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

    public void Initialize()
    {
        if (entryButtons != null && entryButtons.Length > 0)
        {
            foreach (var button in entryButtons)
            {
                Destroy(button);
            }
        }
        entryButtons = AchievementManager.Instance
                .Get()
                .Select((kvp, i) =>
                {
                    if (i == 0) initialEntryId = kvp.Key;

                    var button = Instantiate(entryButtonPrefab, OverviewListUI);
                    var buttonScript = button.GetComponent<AchievementEntryButtonUI>();
                    buttonScript.achievementId = kvp.Key;
                    buttonScript.clickCallback = SetDetails;
                    return button;
                })
                .ToArray();
    }

    public void Show()
    {
        //SFX
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

    public void ToggleAchivements()
    {
        // going from off to on -> update content first
        if (!UI.activeSelf)
        {
            //SFX
            AudioSource.PlayClipAtPoint(BookOpenSFX, Camera.main.transform.position, 1f);

            SetPage(/*root*/);

            //get rid of other pages
            BestiaryPageUI.Instance.UI.SetActive(false);
            SkillsPageUI.Instance.UI.SetActive(false);
        }
        UI.SetActive(!UI.activeSelf);
    }

    public void SetPage(/*AchievementSO achievement*/)
    {
        //SFX
        AudioSource.PlayClipAtPoint(PageTurnSFX, Camera.main.transform.position, 1f);

        SetDetails(initialEntryId);
        SetButtons();


        //change color of sprite based on if youve learnt it
        /*/Image background = image.transform.parent.GetComponent<Image>();
        if (achivement.Progress >= achivement.GoldBar)
        {
            background.color = new UnityEngine.Color(1, 0.8431373f, 1, 1);
        }
        else if (achivement.Progress >= achivement.SilverBar)
        {
            background.color = new UnityEngine.Color(0.7529412f, 0.7529412f, 0.7529412f, 1);
        }
        else if (achivement.Progress >= achivement.BronzeBar)
        {
            background.color = new UnityEngine.Color(0.8039216f, 0.4980392f, 0.1960784f, 1);
        }
        else
        {
            background.color = UnityEngine.Color.gray;
        }

        //set buttons based on if youve learnt that skill
        for (int i = 0; i < Buttons.Length; i++)
        {
            SetAchivementsButtonUI ButtonScript = Buttons[i].GetComponent<SetAchivementsButtonUI>();
            ButtonScript.SetButton();
        }
        removed as there is no longer a background sprite to images, could be implemented somewhere else/*/
    }

    void SetDetails(string achievementId)
    {
        var achievementEntry = AchievementManager.Instance.GetEntry(achievementId);
        if (achievementEntry == null) return;

        //set ui elements to desplay info
        image.sprite = achievementEntry.achievement.achievementIcon;
        achivementTitle.text = achievementEntry.achievement.achievementName;
        description.text = achievementEntry.achievement.description;
        BronzeBar.text = achievementEntry.achievement.levels[0].ToString();
        SilverBar.text = achievementEntry.achievement.levels[1].ToString();
        GoldBar.text = achievementEntry.achievement.levels[2].ToString();
        //progress.text = achievementEntry.progress.ToString();
    }
    void SetButtons()
    {
        foreach (var button in entryButtons)
        {
            var buttonScript = button.GetComponent<AchievementEntryButtonUI>();
            buttonScript.SetButton();
        }
    }

    //gaining/ progressing on achivements
    //[SerializeField] AchievementSO FishCaught;
    //[SerializeField] AchievementSO CommonFishCaught, RareFishCaught, EpicFishCaught;
    //[SerializeField] AchievementSO GroundFishCaught, GrassFishCaught, PlantFishCaught;

    //[SerializeField] AchievementSO PerfectCaught;
    //[SerializeField] AchievementSO BestiaryDescovered, BestiaryMaxSized;

    //[SerializeField] AchievementSO SkillsUnlocked;

    //[SerializeField] AchievementSO HiddenAreaFishing;
    //[SerializeField] HiddenAreaSO[] HiddenAreas;

    public void FishCaughtUpdate(RarityType Rarity, EnvironmentType environment)
    {
        //FishCaught.Progress++;

        //if (Rarity == RarityType.Common)
        //{
        //    CommonFishCaught.Progress++;
        //}
        //else if (Rarity == RarityType.Rare)
        //{
        //    RareFishCaught.Progress++;
        //}
        //else
        //{
        //    EpicFishCaught.Progress++;
        //}

        //if (environment == EnvironmentType.Forest_Ground)
        //{
        //    GroundFishCaught.Progress++;
        //}
        //else if (environment == EnvironmentType.Forest_Undergrowth)
        //{
        //    GrassFishCaught.Progress++;
        //}
        //else if (environment == EnvironmentType.Forest_Plants)
        //{
        //    PlantFishCaught.Progress++;
        //}
    }

    public void PerfectCatchUpdate()
    {
        //PerfectCaught.Progress++;
    }

    public void BestiaryUpdate()
    {
        int descovered = 0;
        int maxsized = 0;
        var bestiaryEntries = BestiaryManager.Instance.Get().Values;
        foreach(var entry in bestiaryEntries)
        {
            if (entry.caughtRarities.Values.Sum() > 0)
            {
                descovered++;
            }

            if (entry.largestSize == entry.catchable.baseSize + entry.catchable.sizeRange)
            {
                maxsized++;
            }
        }
        /*List<CatchableSO> catchables = GameManager.Instance.GetCatchables();
        for (int i = 0; i < catchables.Count; i++)
        {
            BeastiaryEntrySO Bestiary = catchables[i].beastiaryEntry;

            if (Bestiary.CommonCaught + Bestiary.RareCaught + Bestiary.EpicCaught > 0)
            {
                descovered++;
            }

            if (Bestiary.LargestCaught == Bestiary.Fish.baseSize + Bestiary.Fish.sizeRange)
            {
                maxsized++;
            }
        }*/
        //BestiaryDescovered.Progress = descovered;
        //BestiaryMaxSized.Progress = maxsized;
    }

    public void SkillsUpdate()
    {
        //SkillsUnlocked.Progress++;
    }

    public void HiddenFishingUpdate(Vector3Int location)
    {
        //Debug.Log("location is " + location);
        //for (int i = 0;i < HiddenAreas.Count(); i++)
        //{
        //    if (HiddenAreas[i].collected == false & HiddenAreas[i].location == location)
        //    {
        //        Debug.Log("locations we are looking for are " + HiddenAreas[i].location);
        //        HiddenAreas[i].collected = true;
        //        //HiddenAreaFishing.Progress++;
        //        break;
        //    }
        //}
    }
}
