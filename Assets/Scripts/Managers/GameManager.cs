using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static PlayerControls PlayerControls;

    [SerializeField] public MapSectionSO startingScene;

    [SerializeField] CatchableDatabaseSO catchableDatabaseSO;
    [SerializeField] AchievementsDatabaseSO achievementsDatabaseSO;
    [SerializeField] List<HiddenAreaSO> hiddenAreaSOs;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            PlayerControls = new PlayerControls();

        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // first load, register singleton managers that require persistent data
        if(Instance == this)
        {
            DataPersistenceManager.Instance.RegisterSingletonDataPersistenceObjects();
        }
    }

    public void StartNewGame()
    {
        DataPersistenceManager.Instance.NewGame();

        // generate ui lists, etc. from new (empty) data
        //if (BookUI.Instance != null) BookUI.Instance.InitializeManagers();

        MySceneManager.Instance.LoadScene("Intro");
    }
    public void StartLoadedGame()
    {
        DataPersistenceManager.Instance.LoadGame();

        // generate ui lists, etc. from new (loaded) data
        //if (BookUI.Instance != null) BookUI.Instance.InitializeManagers();

        MySceneManager.Instance.LoadScene(startingScene);
    }

    public List<CatchableSO> GetCatchables()
    {
        return catchableDatabaseSO.Entries;
    }
    public List<CatchableSO> GetCatchables(EnvironmentType environment)
    {
        return catchableDatabaseSO.Entries.FindAll(item => item.environments.Contains(environment));
    }
    public CatchableSO GetCatchable(string catchableId)
    {
        if (string.IsNullOrWhiteSpace(catchableId)) return null;

        return catchableDatabaseSO.Entries.Find(item => item.id == catchableId);
    }

    public List<AchievementSO> GetAchievements()
    {
        return achievementsDatabaseSO.Entries;
    }

    public List<HiddenAreaSO> GetHiddenAreas()
    {
        return hiddenAreaSOs;
    }
}
