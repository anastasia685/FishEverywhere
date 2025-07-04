using Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System;

public class BestiaryEntryButtonUI : MonoBehaviour
{
    public BeastiaryEntrySO BeastiaryEntrySO;
    public string catchableId;

    BestiaryEntry bestiaryEntry;

    public Action<string> clickCallback;

    [SerializeField] Image image;

    void Start()
    {
        bestiaryEntry = BestiaryManager.Instance.GetEntry(catchableId);

        if (bestiaryEntry != null)
        {
            image.sprite = bestiaryEntry.catchable.itemIcon;
            SetButton();
        }
    }

    public void SetAsCurrent()
    {
        if(clickCallback != null) clickCallback(catchableId);
    }
    public void SetButton()
    {
        /*if (BeastiaryEntrySO.CommonCaught + BeastiaryEntrySO.RareCaught + BeastiaryEntrySO.EpicCaught == 0)
        {
            image.color = Color.black;
        }*/

        if (bestiaryEntry == null) return;


        if (bestiaryEntry.caughtRarities.Values.Sum() == 0)
        {
            image.color = Color.black;
        }
        else
        {
            image.color = Color.white;
        }
    }
    //SFX
    [SerializeField] AudioClip ButtonPressSFX;
    public void SFX()
    {
        AudioSource.PlayClipAtPoint(ButtonPressSFX, Camera.main.transform.position, 1f);
    }
}
