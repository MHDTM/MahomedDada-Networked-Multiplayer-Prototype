using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGmae()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene("Level 1");
    }

    public void Story()
    {
        SceneManager.LoadScene("Level 1");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

  
