using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuNet : MonoBehaviour
{
    // --- Normal Scene Buttons (optional, for local testing) ---
    public void StartGame()
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

    // --- Networking Buttons ---
    public void Host()
    {
        // If a session is still running (in case player came back to menu), shut it down
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            Debug.Log("⚠️ Previous session still active, shutting down before starting new host...");
            NetworkManager.Singleton.Shutdown();
        }

        if (NetworkManager.Singleton.StartHost())
        {
            Debug.Log("✅ Hosting started successfully. Loading Level 1...");
            NetworkManager.Singleton.SceneManager.LoadScene("Level 1", LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError("❌ Failed to start host!");
        }
    }

    public void StartClient()
    {
        if (NetworkManager.Singleton.StartClient())
        {
            Debug.Log("✅ Client started successfully. Waiting for host to load Level 1...");
            // Client automatically syncs scene with host
        }
        else
        {
            Debug.LogError("❌ Failed to start client!");
        }
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }
}