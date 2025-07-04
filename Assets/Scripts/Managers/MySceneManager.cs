using UnityEngine;
using UnityEngine.SceneManagement;

public class MySceneManager : MonoBehaviour, IDataPersistence, ISkipPersistenceRegistration
{
    public static MySceneManager Instance { get; private set; }

    //[SerializeField] private MapSectionSO startingScene;
    MapSectionSO currentScene;
    string currentMinigame;
    Vector3 playerSpawnPosition;
    Quaternion playerSpawnRotation;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            //currentScene = startingScene;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadData(GameData data)
    {
        playerSpawnPosition = data.position;
    }

    public void SaveData(GameData data)
    {
        // this will be handled by player movement script,
        // since the spawn position is not the immediate position of the player
    }

    public MapSectionSO GetCurrentScene()
    {
        return currentScene;
    }

    public void LoadScene(MapSectionSO scene)
    {
        currentScene = scene;

        LoadScene(scene.sceneName);

        // re-enable player and ui controls when loading a map section scene
        GameManager.PlayerControls.Player.Enable();
        GameManager.PlayerControls.UI.Enable();
    }
    public void LoadScene(string sceneName)
    {
        DataPersistenceManager.Instance.SceneCleanup();

        // disable everything, if it's the map section that's loading, it will re-enable relevant controls
        // if we're loading a scene by its name, means it's title or intro, where input should be blocked
        GameManager.PlayerControls.Disable();

        SceneManager.LoadScene(sceneName);
    }

    public void LoadMinigame(string minigame)
    {
        //if (minigame == currentMinigame) return;

        DataPersistenceManager.Instance.SceneCleanup();

        GameManager.PlayerControls.Player.Disable();
        GameManager.PlayerControls.UI.Disable();

        currentMinigame = minigame;
        SceneManager.LoadScene(minigame);

        GameManager.PlayerControls.MinigamePlayer.Enable();
    }

    public void SwitchScene(CardinalDirection direction)
    {
        MapSectionSO nextScene = null;

        switch (direction)
        {
            case CardinalDirection.Up: 
                nextScene = currentScene.up; 
                playerSpawnPosition = new Vector3(0, 0, -8);
                playerSpawnRotation = Quaternion.LookRotation(Vector3.forward);
                break;
            case CardinalDirection.Down: 
                nextScene = currentScene.down; 
                playerSpawnPosition = new Vector3(0, 0, 8);
                playerSpawnRotation = Quaternion.LookRotation(Vector3.back);
                break;
            case CardinalDirection.Left: 
                nextScene = currentScene.left; 
                playerSpawnPosition = new Vector3(8, 0, 0);
                playerSpawnRotation = Quaternion.LookRotation(Vector3.left);
                break;
            case CardinalDirection.Right: 
                nextScene = currentScene.right; 
                playerSpawnPosition = new Vector3(-8, 0, 0);
                playerSpawnRotation = Quaternion.LookRotation(Vector3.right);
                break;
        }
        if (nextScene != null)
        {
            this.LoadScene(nextScene);
        }
        else
        {
            Debug.LogWarning($"No scene in {direction} direction from {currentScene.sceneName}");
        }
    }

    public Vector3 GetSpawnPosition()
    {
        return playerSpawnPosition;
    }

    public void SetSpawnPosition(Vector3 pos)
    {
        playerSpawnPosition = pos;
    }

    public Quaternion GetSpawnRotation()
    {
        return playerSpawnRotation;
    }

    public void SetSpawnRotation(Quaternion rotation)
    {
        playerSpawnRotation = rotation;
    }
}
