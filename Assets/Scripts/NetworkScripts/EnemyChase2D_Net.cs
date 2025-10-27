using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyChase2D_Net : NetworkBehaviour
{
    public float speed = 2f;
    public float detectionRange = 5f;
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.3f;
    public Transform groundCheckPoint;

    private Rigidbody2D rb;
    private Transform targetPlayer;
    private SpriteRenderer spriteRenderer;
    private float moveDir = 1f;
    private float turnCooldown = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (!IsServer) return; // server authoritative movement

        if (turnCooldown > 0) turnCooldown -= Time.deltaTime;

        bool groundAhead = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckDistance, groundLayer);
        if (!groundAhead && turnCooldown <= 0f)
        {
            moveDir *= -1f;
            turnCooldown = 0.25f;
        }
        else
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            float shortestDistance = Mathf.Infinity;
            targetPlayer = null;

            foreach (var p in players)
            {
                float distance = Vector2.Distance(transform.position, p.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    targetPlayer = p.transform;
                }
            }

            if (targetPlayer != null && shortestDistance <= detectionRange)
            {
                moveDir = Mathf.Sign(targetPlayer.position.x - transform.position.x);
            }
        }

        rb.velocity = new Vector2(moveDir * speed, rb.velocity.y);
        if (moveDir != 0) spriteRenderer.flipX = moveDir > 0;
    }
}