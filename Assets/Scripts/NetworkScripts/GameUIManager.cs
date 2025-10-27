using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public Text localScoreText;
    public Text respawnCountdownText;
    public Slider localHealthSlider;

    public void SetLocalScore(int score, int playerId)
    {
        if (localScoreText != null) localScoreText.text = "Score: " + score;
    }

    public void SetLocalHealth(int health, int playerId)
    {
        if (localHealthSlider != null)
        {
            localHealthSlider.maxValue = 3;
            localHealthSlider.value = health;
        }
    }

    public void StartRespawnCountdown(int playerId, int time)
    {
        if (respawnCountdownText != null)
        {
            respawnCountdownText.gameObject.SetActive(true);
            respawnCountdownText.text = "Respawn in: " + time;
        }
    }

    public void UpdateRespawnCountdown(int playerId, int time)
    {
        if (respawnCountdownText != null)
        {
            respawnCountdownText.text = "Respawn in: " + time;
        }
    }

    public void EndRespawnCountdown(int playerId)
    {
        if (respawnCountdownText != null)
        {
            respawnCountdownText.gameObject.SetActive(false);
        }
    }

}