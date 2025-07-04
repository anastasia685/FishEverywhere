using System;
using UnityEngine;
using UnityEngine.UI;

public class AchievementEntryButtonUI : MonoBehaviour
{
    Image backgroundImage;
    public string achievementId;
    public Action<string> clickCallback;

    Models.AchievementEntry achievementEntry;

    [SerializeField] Image image;

    //SFX
    [SerializeField] AudioClip ButtonPressSFX;

    void Start()
    {
        backgroundImage = GetComponent<Image>();

        achievementEntry = AchievementManager.Instance.GetEntry(achievementId);
        if (achievementEntry != null)
        {
            image.sprite = achievementEntry.achievement.achievementIcon;
            SetButton();
        }
    }

    public void SetButton()
    {
        if (achievementEntry == null) return;

        if (achievementEntry.progress >= achievementEntry.achievement.levels[2])
        {
            backgroundImage.color = new Color(1, 0.8431373f, 0, 1);
        }
        else if (achievementEntry.progress >= achievementEntry.achievement.levels[1])
        {
            backgroundImage.color = new Color(0.7529412f, 0.7529412f, 0.7529412f, 1);
        }
        else if (achievementEntry.progress >= achievementEntry.achievement.levels[0])
        {
            backgroundImage.color = new Color(0.8039216f, 0.4980392f, 0.1960784f, 1);
        }
        else
        {
            backgroundImage.color = Color.gray;
        }
    }

    public void SetAsCurrent()
    {
        if (clickCallback != null) clickCallback(achievementId);
        AudioSource.PlayClipAtPoint(ButtonPressSFX, Camera.main.transform.position, 1f);
    }
}
