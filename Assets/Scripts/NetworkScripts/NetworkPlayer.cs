using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class NetworkPlayer : NetworkBehaviour
{
    [Header("Player Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public int maxHealth = 3;

    [Header("Ground Check")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float checkRadius = 0.15f;

    [Header("Animation State Names (must match Animator)")]
    public string idleAnimName = "Idle";
    public string runningAnimName = "Running";
    public string jumpUpAnimName = "JumpUp";
    public string jumpDownAnimName = "JumpDown";
    public string hitAnimName = "Hit";

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private string currentState;

    [Header("Network Variables")]
    public NetworkVariable<int> Health = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> Score = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // server -> clients: grounded state and velocities (for client-side animation)
    public NetworkVariable<bool> IsGroundedNet = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> HorizontalVelocityNet = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> VerticalVelocityNet = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // server -> clients: facing & animation state
    public NetworkVariable<bool> FacingRight = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> CurrentAnim = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public int PlayerID => (int)OwnerClientId;

    public enum AnimState
    {
        Idle = 0,
        Running = 1,
        JumpUp = 2,
        JumpDown = 3,
        Hit = 4
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Health.Value = maxHealth;
        }

        // UI hooks
        Health.OnValueChanged += OnHealthChanged;
        Score.OnValueChanged += OnScoreChanged;

        // When the server updates facing, clients flip sprite accordingly
        FacingRight.OnValueChanged += (oldVal, newVal) =>
        {
            // FacingRight true => sprite not flipped (assuming default art faces right)
            if (spriteRenderer != null)
                spriteRenderer.flipX = !newVal;
        };

        // When the server updates animation enum, clients play the correct animation
        CurrentAnim.OnValueChanged += (oldVal, newVal) =>
        {
            PlayAnimFromState((AnimState)newVal);
        };

        // When velocities/grounded update we could update blend parameters — but we use discrete states here
        // Ensure initial visual state on spawn
        if (!IsServer)
        {
            FacingRight.Value = FacingRight.Value; // no-op to trigger OnValueChanged in some cases
            CurrentAnim.Value = CurrentAnim.Value;
        }
    }

    public override void OnDestroy()
    {
        Health.OnValueChanged -= OnHealthChanged;
        Score.OnValueChanged -= OnScoreChanged;
    }

    private void Update()
    {
        // Owner handles inputs and sends to server
        if (IsOwner)
        {
            // keep Z locked to zero so clients/host don't get weird z-values
            if (Mathf.Abs(transform.position.z) > 0.001f)
                transform.position = new Vector3(transform.position.x, transform.position.y, 0);

            // Fire handled locally then sent to server
            if (Input.GetButtonDown("Fire1"))
            {
                Vector2 dir = spriteRenderer.flipX ? Vector2.left : Vector2.right;
                FireServerRpc(dir);
            }
        }
    }

    private void FixedUpdate()
    {
        // Owner sends input every physics frame (simple, reliable-ish)
        if (!IsOwner) return;

        float moveInput = Input.GetAxisRaw("Horizontal");
        bool jumpPressed = Input.GetButtonDown("Jump"); // ok to sample here

        SendInputToServerRpc(moveInput, jumpPressed);
    }

    // Server-side: apply movement/physics and update network state variables
    private void HandleServerMovement(float horizontal, bool jumpPressed)
    {
        bool grounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        float newY = rb.velocity.y;

        if (jumpPressed && grounded)
        {
            // apply instant upward velocity
            newY = jumpForce;
        }

        rb.velocity = new Vector2(horizontal * moveSpeed, newY);

        // Update networked state used by clients for visuals
        IsGroundedNet.Value = grounded;
        HorizontalVelocityNet.Value = rb.velocity.x;
        VerticalVelocityNet.Value = rb.velocity.y;

        // Facing
        if (horizontal < -0.01f) FacingRight.Value = false;
        else if (horizontal > 0.01f) FacingRight.Value = true;

        // Decide animation state
        AnimState state = AnimState.Idle;
        if (grounded)
        {
            if (Mathf.Abs(horizontal) > 0.01f) state = AnimState.Running;
            else state = AnimState.Idle;
        }
        else
        {
            if (rb.velocity.y > 0.1f) state = AnimState.JumpUp;
            else if (rb.velocity.y < -0.1f) state = AnimState.JumpDown;
        }

        CurrentAnim.Value = (int)state;
    }

    // Owner -> Server RPC: send simple input
    [ServerRpc(RequireOwnership = true)]
    private void SendInputToServerRpc(float horizontal, bool jumpPressed, ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;
        HandleServerMovement(horizontal, jumpPressed);
    }

    // Server-spawned projectile (server authoritative)
    [ServerRpc(RequireOwnership = false)]
    private void FireServerRpc(Vector2 dir, ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;
        var client = NetworkManager.Singleton.ConnectedClients[OwnerClientId];
        if (client == null || client.PlayerObject == null) return;

        Vector3 spawnPos = client.PlayerObject.transform.position + (Vector3)(dir * 0.5f);
        NetworkGameManager.Instance?.SpawnProjectileServer(spawnPos, dir);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int amount, ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;

        Health.Value -= amount;
        if (Health.Value <= 0)
        {
            Health.Value = 0;
            NetworkGameManager.Instance?.HandlePlayerDied(OwnerClientId);
        }
        else
        {
            CurrentAnim.Value = (int)AnimState.Hit;
        }
    }

    [ServerRpc]
    public void AddScoreServerRpc(int amount, ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;
        Score.Value += amount;
    }

    private void PlayAnimFromState(AnimState st)
    {
        // Called on clients when CurrentAnim changes
        string animName = idleAnimName;
        switch (st)
        {
            case AnimState.Running: animName = runningAnimName; break;
            case AnimState.JumpUp: animName = jumpUpAnimName; break;
            case AnimState.JumpDown: animName = jumpDownAnimName; break;
            case AnimState.Hit: animName = hitAnimName; break;
            case AnimState.Idle:
            default: animName = idleAnimName; break;
        }
        ChangeAnimationState(animName);
    }

    private void ChangeAnimationState(string newState)
    {
        if (animator == null || currentState == newState) return;
        animator.Play(newState);
        currentState = newState;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;

        if (collision.CompareTag("Enemy") || collision.CompareTag("Damage"))
            TakeDamageServerRpc(1);

        if (collision.CompareTag("Damage layer"))
            TakeDamageServerRpc(3);
    }

    private void OnHealthChanged(int oldVal, int newVal)
    {
        if (IsOwner)
        {
            var ui = FindObjectOfType<GameUIManager>();
            if (ui != null) ui.SetLocalHealth(newVal, PlayerID);
        }
    }

    private void OnScoreChanged(int oldVal, int newVal)
    {
        if (IsOwner)
        {
            var ui = FindObjectOfType<GameUIManager>();
            if (ui != null) ui.SetLocalScore(newVal, PlayerID);
        }
    }
}