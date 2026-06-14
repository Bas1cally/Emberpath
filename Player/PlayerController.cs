using UnityEngine;

/// <summary>
/// Emberpath - Core Player Movement Controller (Prototype v0.1)
/// Handles: Run, Jump, Double Jump, Dash, Ground/Wall checks.
/// Attach to the player GameObject together with a Rigidbody2D and Collider2D.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Horizontal movement speed in units/second")]
    public float moveSpeed = 8f;

    [Header("Jump")]
    [Tooltip("Initial upward velocity applied on jump")]
    public float jumpForce = 14f;
    [Tooltip("Extra multiplier applied to gravity when falling, for snappier jumps")]
    public float fallGravityMultiplier = 2.2f;
    [Tooltip("How many jumps the player can perform before touching ground again (2 = single + double jump)")]
    public int maxJumps = 2;

    [Header("Dash")]
    [Tooltip("Speed applied during a dash")]
    public float dashSpeed = 20f;
    [Tooltip("Duration of the dash in seconds")]
    public float dashDuration = 0.15f;
    [Tooltip("Cooldown before another dash can be performed")]
    public float dashCooldown = 0.6f;
    [Tooltip("If true, dash grants brief invulnerability (i-frames) - useful for testing dodge timing")]
    public bool dashGrantsIFrames = true;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    // --- internal state ---
    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;
    private int jumpsRemaining;
    private bool jumpQueued;

    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    private float dashDirection;

    public bool IsInvulnerable { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        ReadInput();
        UpdateTimers();
    }

    private void ReadInput()
    {
        // Placeholder input - swap for Touch/UI buttons or new Input System later.
        moveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
        {
            jumpQueued = true;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && dashCooldownTimer <= 0f && !isDashing)
        {
            StartDash();
        }
    }

    private void UpdateTimers()
    {
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                EndDash();
            }
        }
    }

    private void FixedUpdate()
    {
        CheckGrounded();

        if (isDashing)
        {
            // During a dash, override normal movement with a fixed dash velocity.
            rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);
            return;
        }

        ApplyHorizontalMovement();
        ApplyJump();
        ApplyFallGravity();
    }

    private void CheckGrounded()
    {
        bool wasGrounded = isGrounded;

        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        if (isGrounded && !wasGrounded)
        {
            jumpsRemaining = maxJumps;
        }
        else if (!wasGrounded && jumpsRemaining == 0 && isGrounded)
        {
            jumpsRemaining = maxJumps;
        }

        // First-frame initialization safeguard.
        if (isGrounded && jumpsRemaining == 0 && rb.linearVelocity.y <= 0.01f)
        {
            jumpsRemaining = maxJumps;
        }
    }

    private void ApplyHorizontalMovement()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // Flip sprite based on movement direction.
        if (moveInput != 0f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(moveInput) * Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    private void ApplyJump()
    {
        if (!jumpQueued)
        {
            return;
        }

        jumpQueued = false;

        if (jumpsRemaining > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpsRemaining--;
        }
    }

    private void ApplyFallGravity()
    {
        // Makes falling feel snappier than rising - classic platformer "game feel" trick.
        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallGravityMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }

    private void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

        // Dash in current facing direction, or input direction if pressed.
        dashDirection = moveInput != 0f ? Mathf.Sign(moveInput) : Mathf.Sign(transform.localScale.x);

        if (dashGrantsIFrames)
        {
            IsInvulnerable = true;
        }
    }

    private void EndDash()
    {
        isDashing = false;
        IsInvulnerable = false;

        // Reduce velocity after dash so it doesn't feel like an infinite slide.
        rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.3f, rb.linearVelocity.y);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
