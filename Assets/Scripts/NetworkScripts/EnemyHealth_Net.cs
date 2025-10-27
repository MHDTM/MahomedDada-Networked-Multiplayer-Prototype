using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EnemyHealth_Net : NetworkBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

    private void Start()
    {
        if (IsServer) currentHealth = maxHealth;
    }

    // server-only damage
    public void TakeDamageServer(int amount)
    {
        if (!IsServer) return;
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            // despawn/destroy on server
            var no = GetComponent<NetworkObject>();
            if (no != null) no.Despawn(true);
            Destroy(gameObject);
        }
    }
}