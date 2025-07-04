using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    public void OnContinueClicked()
    {
        GameManager.Instance.StartLoadedGame();
    }
    public void OnNewGameClicked()
    {
        GameManager.Instance.StartNewGame();
    }
    public void OnQuitClicked()
    {
        Application.Quit();
    }
}
