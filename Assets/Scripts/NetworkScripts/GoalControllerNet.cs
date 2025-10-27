using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GoalControllerNet : NetworkBehaviour
{
    private HashSet<ulong> playersInZone = new HashSet<ulong>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Player"))
        {
            var netPlayer = other.GetComponent<NetworkPlayer>();
            if (netPlayer != null)
            {
                playersInZone.Add(netPlayer.OwnerClientId);
                CheckVictory();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsServer) return;
        if (other.CompareTag("Player"))
        {
            var netPlayer = other.GetComponent<NetworkPlayer>();
            if (netPlayer != null)
                playersInZone.Remove(netPlayer.OwnerClientId);
        }
    }

    private void CheckVictory()
    {
        // modify condition as needed (>=2 ensures two players present)
        if (playersInZone.Count >= 2)
        {
            // use Netcode scene manager to load scene for everyone
            NetworkManager.Singleton.SceneManager.LoadScene("VictoryScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
}