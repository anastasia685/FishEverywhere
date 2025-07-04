using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FishCatchingUI : MonoBehaviour
{
    public GameObject UI;

    public Image backgroundImg;
    public Image fishImg;
    public Text fishName;
    public Text rarity;
    public Image rarityBar;
    public Text size;

    public static FishCatchingUI Instance;

    //SFX
    [SerializeField] AudioClip RaritySFX;
    [SerializeField] AudioClip ImgSFX;
    AudioSource SizeSFX;

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

    public void StartAnimation(CatchableSO Fish, RarityType Rarity, int Size)
    {
        //SFX
        if (SizeSFX == null) SizeSFX = GetComponent<AudioSource>();

        StartCoroutine(CatchingUI(Fish, Rarity, Size));
    }
    public IEnumerator CatchingUI(CatchableSO Fish, RarityType Rarity, int Size)
    {
        //send infomation to other Ui elements
        InventoryManager.Instance.AddItem(Fish, RewardManager.Instance.GetPendingRarity(), RewardManager.Instance.GetPendingSize());
        BestiaryManager.Instance.UpdateEntry(Fish.id, RewardManager.Instance.GetPendingRarity(), RewardManager.Instance.GetPendingSize());
        SkillManager.Instance.IncrementPoints(RewardManager.Instance.GetPendingSkillPoints());
        //AchievementsPageUI.Instance.FishCaughtUpdate(RewardManager.Instance.GetPendingRarity(), RewardManager.Instance.GetPendingEnvironment());
        AchievementManager.Instance.OnFishCaught(RewardManager.Instance.GetPendingRarity(), RewardManager.Instance.GetPendingEnvironment());
        RewardManager.Instance.ClearPendingReward();

        //bring player to world scene
        MySceneManager.Instance.LoadScene(MySceneManager.Instance.GetCurrentScene());

        //disable player controls
        yield return null;
        GameManager.PlayerControls.Disable();

        //sets UI to be visable
        UI.SetActive(true);

        //set ui to variables
        fishImg.sprite = Fish.itemIcon;
        fishName.text = Fish.itemName;

        //set most ui invisable 
        backgroundImg.gameObject.SetActive(false);
        fishName.gameObject.SetActive(false);
        rarity.gameObject.SetActive(false);
        rarityBar.gameObject.SetActive(false);
        size.gameObject.SetActive(false);

        //set img to small, then scale into normal sized, rotate at the same time
        fishImg.transform.localScale = new Vector3(0, 0, 0);
        for (float i = 0f; i < 1f; i += Time.deltaTime / 2)
        {
            fishImg.transform.localScale = new Vector3(i, i, i);
            fishImg.transform.localRotation = Quaternion.Euler(0, i * 720, 0);
            yield return null;
        }
        fishImg.transform.localScale = new Vector3(1, 1, 1);
        fishImg.transform.localRotation = Quaternion.identity;

        //SFX
        AudioSource.PlayClipAtPoint(ImgSFX, Camera.main.transform.position, 1f);

        yield return new WaitForSeconds(0.5f);

        //set visability
        backgroundImg.gameObject.SetActive(true);
        fishName.gameObject.SetActive(true);
        rarity.gameObject.SetActive(true);
        rarityBar.gameObject.SetActive(true);

        //go up rarity scale
        rarity.text = "Common";
        rarityBar.color = UnityEngine.Color.white;
        if (Rarity > RarityType.Common)
        {
            yield return new WaitForSeconds(1);
            rarityBar.transform.localScale = new Vector3 (0.8f, 1, 1);
            yield return new WaitForSeconds(0.5f);
            rarityBar.transform.localScale = new Vector3(1, 1, 1);
            rarity.text = "Rare";
            rarityBar.color = new UnityEngine.Color(0.3226415f, 0.9185385f, 1, 1);

            //SFX
            AudioSource.PlayClipAtPoint(RaritySFX, Camera.main.transform.position, 1f);

            if (Rarity > RarityType.Rare)
            {
                yield return new WaitForSeconds(1);
                rarityBar.transform.localScale = new Vector3(0.8f, 1, 1);
                yield return new WaitForSeconds(0.5f);
                rarityBar.transform.localScale = new Vector3(1, 1, 1);
                rarity.text = "Epic";
                rarityBar.color = new UnityEngine.Color(0.8433578f, 0.3603774f, 1, 1);

                //SFX
                AudioSource.PlayClipAtPoint(RaritySFX, Camera.main.transform.position, 1f);

                if (Rarity > RarityType.Epic)
                {
                    yield return new WaitForSeconds(1);
                    rarityBar.transform.localScale = new Vector3(0.8f, 1, 1);
                    yield return new WaitForSeconds(0.5f);
                    rarityBar.transform.localScale = new Vector3(1, 1, 1);
                    rarity.text = "Legendary";
                    rarityBar.color = new UnityEngine.Color(255, 255, 0);

                    //SFX
                    AudioSource.PlayClipAtPoint(RaritySFX, Camera.main.transform.position, 1f);

                    if (Rarity > RarityType.Legendary)
                {
                    yield return new WaitForSeconds(1);
                    rarityBar.transform.localScale = new Vector3(0.8f, 1, 1);
                    yield return new WaitForSeconds(0.5f);
                    rarityBar.transform.localScale = new Vector3(1, 1, 1);
                    rarity.text = "Mythic";
                    rarityBar.color = new UnityEngine.Color(255, 0, 0);

                        //SFX
                        AudioSource.PlayClipAtPoint(RaritySFX, Camera.main.transform.position, 1f);
                    }
                }
            }
        }

        yield return new WaitForSeconds(0.5f);
        //set visibility of size text
        size.gameObject.SetActive(true);

        //SFX
        SizeSFX.Play();

        float waitTime = 0.15f;
        for (int i = 0; i < Size; i++)
        {
            size.text = i + " cm";
            waitTime *= 0.975f;
            yield return new WaitForSeconds(waitTime);
        }
        size.text = Size + " cm";

        //SFX
        SizeSFX.Stop();

        yield return new WaitForSeconds(1.5f);
        //sets UI to be invisable
        UI.SetActive(false);

        // enable controls
        GameManager.PlayerControls.Enable();
    }
}
