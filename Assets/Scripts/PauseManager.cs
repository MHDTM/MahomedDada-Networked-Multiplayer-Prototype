using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;


public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuUI;
    private bool isPaused = false;
    private NetworkPlayer playerController;

    void Start()
    {
        playerController = GetComponent<NetworkPlayer>();
        pauseMenuUI.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        pauseMenuUI.SetActive(isPaused);

        if (playerController != null)
        {
            playerController.enabled = !isPaused; // Disable input while paused
        }

        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;
    }

    public void ResumeGame()
    {
        TogglePause(); // just call this from Resume button
    }

    public void ReturnToMainMenu()
    {
        // Cleanly shut down networking before loading menu
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            Debug.Log("🧹 Shutting down network before returning to Main Menu...");
            NetworkManager.Singleton.Shutdown();
        }

        // Unlock and show cursor for menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene("Main Menu");
    }

}