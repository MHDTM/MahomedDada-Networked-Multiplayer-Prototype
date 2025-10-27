using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndScreenManager : MonoBehaviour
{
    public Text p1Text;
    public Text p2Text;
    public Text totalText;

    void Start()
    {
        int p1 = PlayerPrefs.GetInt("P1FinalScore", 0);
        int p2 = PlayerPrefs.GetInt("P2FinalScore", 0);
        int total = PlayerPrefs.GetInt("TotalFinalScore", p1 + p2);

        if (p1Text) p1Text.text = "P1 Final Score: " + p1;
        if (p2Text) p2Text.text = "P2 Final Score: " + p2;
        if (totalText) totalText.text = "Total: " + total;
    }

    public void RestartGame() { SceneManager.LoadScene("Level 1"); }
    public void QuitGame() { Application.Quit(); }
    public void MainMenu() { SceneManager.LoadScene(0); }
}