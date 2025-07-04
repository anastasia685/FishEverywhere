using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BookUI : MonoBehaviour
{
    public static BookUI Instance { get; private set; }

    [SerializeField] GameObject canvas;

    IBookPageUI activePage;

    Caster caster;

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

    void Start()
    {
        InitializeManagers();
        activePage = BestiaryPageUI.Instance;
    }

    void BookUIClicked(InputAction.CallbackContext ctx)
    {
        if (!canvas.activeSelf)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    void Show()
    {
        if (PauseMenuUI.Instance) PauseMenuUI.Instance.Hide();

        caster = FindFirstObjectByType<Caster>();
        if (caster != null) caster.CancelCast();

        GameManager.PlayerControls.Player.Disable();

        activePage.Show();

        canvas.SetActive(true);
    }
    public void Hide()
    {
        activePage.Hide();
        activePage = BestiaryPageUI.Instance;

        GameManager.PlayerControls.Player.Enable();

        canvas.SetActive(false);
    }

    void OnEnable()
    {
        if (Instance != this) return;

        GameManager.PlayerControls.UI.Bestiary.performed += BookUIClicked;
    }
    void OnDisable()
    {
        if (Instance != this) return;

        GameManager.PlayerControls.UI.Bestiary.performed -= BookUIClicked;
    }

    public void InitializeManagers()
    {
        // after the data is loaded, allow other managers to fetch data from idatapersistence managers
        BestiaryPageUI.Instance.Initialize();
        SkillsPageUI.Instance.Initialize();
        AchievementsPageUI.Instance.Initialize();
    }

    public void SetActivePage(IBookPageUI page)
    {
        activePage.Hide();
        activePage = page;
        activePage.Show();
    }
}
