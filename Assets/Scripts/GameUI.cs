using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    public void ReloadGame()
    {
        GameManager.Instance.StartGame();
    }

    public void ExitGame()
    {
        GameManager.Instance.QuitGame();
    }
}
