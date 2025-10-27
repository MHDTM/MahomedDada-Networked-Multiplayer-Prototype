using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkGameManager : NetworkBehaviour
{
    public static NetworkGameManager Instance;

    [Header("References")]
    public GameObject projectilePrefab;
    public Transform[] spawnPoints; // assign in inspector: spawn point 0 -> player slot 1, spawn point 1 -> slot 2
    public AudioClip enemyDeathClip, throwClip;

    [Header("Gameplay")]
    public int respawnDelay = 5;

    [Header("Audio Clips")]
    public AudioSource sfxSource;
    public AudioClip jumpClip;
    public AudioClip collectClip;

    // Server-only connected clients order = slot assignment
    private List<ulong> connectedClients = new List<ulong>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Assign a slot to the client and teleport to spawn
    public void AssignSlotToClient(ulong clientId)
    {
        if (!IsServer) return;

        if (!connectedClients.Contains(clientId))
            connectedClients.Add(clientId);

        int slot = connectedClients.IndexOf(clientId) + 1; // 1-based

        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            var client = NetworkManager.Singleton.ConnectedClients[clientId];
            if (client.PlayerObject != null)
            {
                var player = client.PlayerObject.GetComponent<NetworkPlayer>();
                if (player != null)
                {
                    // PlayerID is already handled in NetworkPlayer as a property
                    // Teleport player to spawn point
                    if (slot - 1 < spawnPoints.Length)
                    {
                        Vector3 spawnPos = spawnPoints[slot - 1].position;
                        spawnPos.z = 0;
                        client.PlayerObject.transform.SetPositionAndRotation(spawnPos, Quaternion.identity);

                        // Force network sync
                        var netTransform = client.PlayerObject.GetComponent<Unity.Netcode.Components.NetworkTransform>();
                        if (netTransform != null)
                        {
                            netTransform.Teleport(spawnPos, Quaternion.identity, Vector3.one);
                        }
                    }
                }
            }
        }
    }

    // Spawn projectile server-side
    public void SpawnProjectileServer(Vector3 spawnPos, Vector2 dir)
    {
        if (!IsServer) return;

        GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        projectile.GetComponent<NetworkObject>().Spawn(true);

        var proj = projectile.GetComponent<NetworkProjectile>();
        if (proj != null) proj.SetDirectionServer(dir);
    }

    // Handle player death
    public void HandlePlayerDied(ulong clientId)
    {
        if (!IsServer) return;

        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
            return;

        var client = NetworkManager.Singleton.ConnectedClients[clientId];
        if (client.PlayerObject == null) return;

        var player = client.PlayerObject.GetComponent<NetworkPlayer>();
        if (player == null) return;

        // Disable player
        player.gameObject.SetActive(false);

        // Start respawn coroutine
        StartCoroutine(RespawnCoroutine(player));
    }

    private IEnumerator RespawnCoroutine(NetworkPlayer player)
    {
        int countdown = respawnDelay;
        var ui = FindObjectOfType<GameUIManager>();

        // Show countdown UI if exists
        if (ui != null)
            ui.StartRespawnCountdown(player.PlayerID, countdown);

        while (countdown > 0)
        {
            yield return new WaitForSeconds(1f);
            countdown--;

            if (ui != null)
                ui.UpdateRespawnCountdown(player.PlayerID, countdown);
        }

        // Teleport to spawn
        int slot = player.PlayerID;
        Transform spawnPoint = (slot - 1 >= 0 && slot - 1 < spawnPoints.Length) ? spawnPoints[slot - 1] : null;
        Vector3 respawnPos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        respawnPos.z = 0;
        player.transform.position = respawnPos;

        // Reset velocity
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.velocity = Vector2.zero;

        // Restore health
        player.Health.Value = player.maxHealth;

        // Hide countdown UI
        if (ui != null)
            ui.EndRespawnCountdown(player.PlayerID);

        // Force network sync
        var netTransform = player.GetComponent<Unity.Netcode.Components.NetworkTransform>();
        if (netTransform != null)
            netTransform.Teleport(respawnPos, Quaternion.identity, Vector3.one);

        // Re-enable player
        player.gameObject.SetActive(true);
    }

    // Client RPC to play jump sound
    [ClientRpc]
    public void PlayJumpAudioClientRpc()
    {
        if (sfxSource && jumpClip) sfxSource.PlayOneShot(jumpClip);
    }

    // Client RPC to play collect sound
    [ClientRpc]
    public void PlayCollectAudioClientRpc()
    {
        if (sfxSource && collectClip) sfxSource.PlayOneShot(collectClip);
    }
}