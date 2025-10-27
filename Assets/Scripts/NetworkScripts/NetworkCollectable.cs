using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject))]
public class NetworkCollectable : NetworkBehaviour
{
    public int value = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;

        var player = other.GetComponent<NetworkPlayer>();
        if (player != null)
        {
            player.AddScoreServerRpc(value);
            // optional sound
            NetworkGameManager.Instance?.PlayCollectAudioClientRpc(); // or separate Collect RPC
            GetComponent<NetworkObject>()?.Despawn(true);
        }
    }
}