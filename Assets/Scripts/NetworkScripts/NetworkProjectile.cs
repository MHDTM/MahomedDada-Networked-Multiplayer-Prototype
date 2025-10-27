using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody2D), typeof(NetworkObject))]
public class NetworkProjectile : NetworkBehaviour
{
    [Header("Projectile Settings")]
    public int damage = 1;
    public float speed = 10f;
    public float lifetime = 5f;

    private Vector2 direction;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            StartCoroutine(DespawnAfterTime());
    }

    // Called by server immediately after spawn
    public void SetDirectionServer(Vector2 dir)
    {
        if (!IsServer) return;
        direction = dir.normalized;
        rb.velocity = direction * speed; // ✅ apply velocity immediately
    }

    private IEnumerator DespawnAfterTime()
    {
        yield return new WaitForSeconds(lifetime);
        if (IsServer && GetComponent<NetworkObject>().IsSpawned)
            GetComponent<NetworkObject>().Despawn(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Player")) return; // don’t hit players

        if (other.CompareTag("Enemy"))
        {
            var eh = other.GetComponent<EnemyHealth_Net>();
            if (eh != null) eh.TakeDamageServer(damage);
        }

        if (GetComponent<NetworkObject>().IsSpawned)
            GetComponent<NetworkObject>().Despawn(true);
    }
}