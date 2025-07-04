using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public interface ISkipPersistenceRegistration { };

public class DataPersistenceManager : MonoBehaviour
{
    public static DataPersistenceManager Instance { get; private set; }

    [SerializeField] string fileName;

    GameData gameData;
    List<IDataPersistence> dataPersistenceObjects;
    List<IDataPersistence> singletonDataPersistenceObjects;
    FileDataHandler dataHandler;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnApplicationQuit()
    {
        //SaveGame();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        SceneSetup();
    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // should be done on scene load
    public void SceneSetup()
    {
        this.dataPersistenceObjects = FindSceneDataPersistenceObjects();

        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.LoadData(gameData);
        }
    }
    // should be done on scene unload (before loading the next scene in scenemanager
    public void SceneCleanup()
    {
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            if (dataPersistenceObj == null) continue;

            dataPersistenceObj.SaveData(gameData);
        }
    }

    public void NewGame()
    {
        this.gameData = new GameData();

        foreach (IDataPersistence dataPersistenceObj in singletonDataPersistenceObjects)
        {
            if (dataPersistenceObj == null) continue;
            dataPersistenceObj.LoadData(gameData);
        }
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            if (dataPersistenceObj == null) continue;
            dataPersistenceObj.LoadData(gameData);
        }

        // call save game to write new clean savefile to disk
        dataHandler.Save(gameData);
    }

    public void LoadGame()
    {
        this.gameData = dataHandler.Load();

        if(this.gameData == null)
        {
            NewGame();
        }

        foreach (IDataPersistence dataPersistenceObj in singletonDataPersistenceObjects)
        {
            if (dataPersistenceObj == null) continue;
            dataPersistenceObj.LoadData(gameData);
        }
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            if (dataPersistenceObj == null) continue;
            dataPersistenceObj.LoadData(gameData);
        }
    }

    public void SaveGame()
    {
        // save singleton classes' data
        foreach (IDataPersistence dataPersistenceObj in singletonDataPersistenceObjects)
        {
            if (dataPersistenceObj == null) continue;

            dataPersistenceObj.SaveData(gameData);
        }
        // save regular game objects' data
        SceneCleanup();

        // write to file
        dataHandler.Save(gameData);
    }

    List<IDataPersistence> FindSceneDataPersistenceObjects()
    {
        var dataPersistenceObjects = FindObjectsByType(typeof(MonoBehaviour), FindObjectsInactive.Include, FindObjectsSortMode.None)
            .OfType<IDataPersistence>()
            .Where(o => o is not ISkipPersistenceRegistration)
            .ToList();
        return dataPersistenceObjects;
    }

    public void RegisterSingletonDataPersistenceObjects()
    {
        this.singletonDataPersistenceObjects = 
            FindObjectsByType(typeof(MonoBehaviour), FindObjectsInactive.Include, FindObjectsSortMode.None)
            .OfType<ISkipPersistenceRegistration>()
            .OfType<IDataPersistence>()
            .ToList();
    }
}
