using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] GameObject canvas;
    Caster caster;

    public static PauseMenuUI Instance;

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

    void Show()
    {
        Time.timeScale = 0;

        if (BookUI.Instance) BookUI.Instance.Hide();

        caster = FindFirstObjectByType<Caster>();
        if (caster != null) caster.CancelCast();

        GameManager.PlayerControls.Player.Disable();
        canvas.SetActive(true);
    }
    public void Hide()
    {
        Time.timeScale = 1;

        GameManager.PlayerControls.Player.Enable();
        canvas.SetActive(false);
    }

    void PauseClicked(InputAction.CallbackContext ctx)
    {
        if(!canvas.activeSelf)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    void OnEnable()
    {
        GameManager.PlayerControls.UI.PauseMenu.performed += PauseClicked;
    }
    void OnDisable()
    {
        GameManager.PlayerControls.UI.PauseMenu.performed -= PauseClicked;
    }

    public void OnSaveClicked()
    {
        DataPersistenceManager.Instance.SaveGame();
    }

    public void OnToTitleClicked()
    {
        MySceneManager.Instance.LoadScene("Title");
        Hide();
    }
}
