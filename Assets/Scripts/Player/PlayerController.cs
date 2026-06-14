using Emberpath.Core;
using UnityEngine;

namespace Emberpath.Player
{
    /// <summary>
    /// Rigidbody2D-driven platformer movement for the vertical slice:
    /// run, double jump and dash, with coyote time and jump buffering so the
    /// controls feel forgiving. Uses the legacy Input Manager so it works on a
    /// fresh project without configuring an input asset; touch controls come later.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Run")]
        [Tooltip("Target horizontal speed in units/second.")]
        [SerializeField] private float moveSpeed = 8f;
        [Tooltip("How quickly the player reaches target speed on the ground.")]
        [SerializeField] private float groundAcceleration = 90f;
        [Tooltip("How quickly the player changes speed in the air.")]
        [SerializeField] private float airAcceleration = 45f;

        [Header("Jump")]
        [SerializeField] private float jumpForce = 15f;
        [Tooltip("Extra mid-air jumps after the grounded jump. 0 = single jump only; " +
                 "set to 1 later to unlock the double jump.")]
        [SerializeField] private int extraJumps = 0;
        [Tooltip("Upward velocity is cut by this factor when the jump button is released early.")]
        [Range(0f, 1f)]
        [SerializeField] private float jumpCutMultiplier = 0.5f;
        [Tooltip("Gravity multiplier while falling, for a snappier arc.")]
        [SerializeField] private float fallGravityMultiplier = 2.0f;
        [Tooltip("Seconds after leaving a ledge during which a jump is still allowed.")]
        [SerializeField] private float coyoteTime = 0.1f;
        [Tooltip("Seconds a jump press is remembered before landing.")]
        [SerializeField] private float jumpBufferTime = 0.1f;

        [Header("Dash")]
        [SerializeField] private float dashSpeed = 22f;
        [SerializeField] private float dashDuration = 0.15f;
        [SerializeField] private float dashCooldown = 0.5f;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.18f;
        [SerializeField] private LayerMask groundLayer;

        public bool IsDashing { get; private set; }
        public int FacingDirection { get; private set; } = 1;

        /// <summary>
        /// Wires up references that are normally assigned in the inspector.
        /// Used by <c>TestArenaBootstrap</c> when it builds the player at runtime.
        /// Call before the GameObject is activated for it to take effect in Awake.
        /// </summary>
        public void ConfigureReferences(Transform groundCheckTransform, LayerMask groundMask)
        {
            if (groundCheckTransform != null) groundCheck = groundCheckTransform;
            groundLayer = groundMask;
        }

        /// <summary>Overrides the movement feel values from a shared tuning object.</summary>
        public void ApplyTuning(PlayerTuning t)
        {
            if (t == null) return;
            moveSpeed = t.moveSpeed;
            jumpForce = t.jumpForce;
            extraJumps = t.extraJumps;
            fallGravityMultiplier = t.fallGravityMultiplier;
            dashSpeed = t.dashSpeed;
            dashDuration = t.dashDuration;
            dashCooldown = t.dashCooldown;
        }

        private Rigidbody2D _rb;
        private SpriteRenderer _sprite;
        private Color _baseColor = Color.white;
        private readonly Color _dashColor = new Color(0.5f, 0.9f, 1f);
        private float _defaultGravityScale;

        private float _moveInput;
        private bool _isGrounded;
        private int _jumpsRemaining;

        private float _coyoteCounter;
        private float _jumpBufferCounter;
        private float _jumpLockTimer;
        // After a jump, ignore ground for this long so the still-overlapping foot
        // check can't immediately re-ground and hand out a second jump.
        private const float JumpLockTime = 0.08f;

        private float _dashTimeLeft;
        private float _dashCooldownLeft;
        private float _dashDirection;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _sprite = GetComponentInChildren<SpriteRenderer>();
            if (_sprite != null) _baseColor = _sprite.color;
            _defaultGravityScale = _rb.gravityScale;
            _rb.freezeRotation = true;

            if (groundCheck == null)
            {
                // Fall back to a point at the player's feet so the controller still
                // works if the GroundCheck child was not assigned in the inspector.
                groundCheck = transform;
            }
        }

        private void Update()
        {
            ReadInput();
            UpdateTimers();
            UpdateFacing();
        }

        private void FixedUpdate()
        {
            CheckGrounded();

            if (IsDashing)
            {
                TickDash();
                return;
            }

            ApplyHorizontalMovement();
            HandleJump();
            ApplyBetterGravity();
        }

        private void ReadInput()
        {
            _moveInput = Input.GetAxisRaw("Horizontal");

            if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                _jumpBufferCounter = jumpBufferTime;
            }

            if (Input.GetButtonUp("Jump") || Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow))
            {
                // Variable jump height: cut the rise short on early release.
                if (_rb.linearVelocity.y > 0f)
                {
                    _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _rb.linearVelocity.y * jumpCutMultiplier);
                }
            }

            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.JoystickButton2))
            {
                TryStartDash();
            }
        }

        private void UpdateTimers()
        {
            _coyoteCounter -= Time.deltaTime;
            _jumpBufferCounter -= Time.deltaTime;
            _jumpLockTimer -= Time.deltaTime;
            _dashCooldownLeft -= Time.deltaTime;
        }

        private void UpdateFacing()
        {
            if (IsDashing) return;

            if (_moveInput > 0.01f) FacingDirection = 1;
            else if (_moveInput < -0.01f) FacingDirection = -1;

            if (_sprite != null)
            {
                _sprite.flipX = FacingDirection < 0;
            }
        }

        private void CheckGrounded()
        {
            // Detect ground but never count our own body/children as ground — otherwise
            // the foot-level check overlaps the player's own collider and reports
            // "grounded" forever, which allows infinite jumps. This also keeps working
            // if the ground LayerMask is misconfigured (e.g. the Ground layer is missing).
            _isGrounded = false;

            // Don't re-ground during the brief post-jump lock.
            if (_jumpLockTimer > 0f) return;

            Collider2D[] hits = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, groundLayer);
            foreach (Collider2D c in hits)
            {
                if (c == null) continue;
                if (c.attachedRigidbody == _rb) continue;       // our own rigidbody
                if (c.transform.IsChildOf(transform)) continue; // our own children
                _isGrounded = true;
                break;
            }

            if (_isGrounded)
            {
                _coyoteCounter = coyoteTime;
                _jumpsRemaining = extraJumps;
            }
        }

        private void ApplyHorizontalMovement()
        {
            float targetSpeed = _moveInput * moveSpeed;
            float accel = _isGrounded ? groundAcceleration : airAcceleration;
            float newX = Mathf.MoveTowards(_rb.linearVelocity.x, targetSpeed, accel * Time.fixedDeltaTime);
            _rb.linearVelocity = new Vector2(newX, _rb.linearVelocity.y);
        }

        private void HandleJump()
        {
            if (_jumpBufferCounter <= 0f) return;

            bool canGroundJump = _coyoteCounter > 0f;
            bool canAirJump = !canGroundJump && _jumpsRemaining > 0;

            if (!canGroundJump && !canAirJump) return;

            if (canAirJump) _jumpsRemaining--;

            // Reset vertical velocity first so jump height is consistent whether
            // rising, falling or at apex.
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);

            _jumpBufferCounter = 0f;
            _coyoteCounter = 0f;
            _jumpLockTimer = JumpLockTime;
        }

        private void ApplyBetterGravity()
        {
            // Heavier gravity on the way down keeps jumps from feeling floaty.
            _rb.gravityScale = _rb.linearVelocity.y < 0f
                ? _defaultGravityScale * fallGravityMultiplier
                : _defaultGravityScale;
        }

        private void TryStartDash()
        {
            if (IsDashing || _dashCooldownLeft > 0f) return;

            IsDashing = true;
            _dashTimeLeft = dashDuration;
            _dashCooldownLeft = dashCooldown;
            _dashDirection = Mathf.Abs(_moveInput) > 0.01f ? Mathf.Sign(_moveInput) : FacingDirection;

            // Visible tint so the dash reads clearly even with placeholder art.
            if (_sprite != null) _sprite.color = _dashColor;
        }

        private void TickDash()
        {
            _rb.gravityScale = 0f;
            _rb.linearVelocity = new Vector2(_dashDirection * dashSpeed, 0f);

            _dashTimeLeft -= Time.fixedDeltaTime;
            if (_dashTimeLeft <= 0f)
            {
                IsDashing = false;
                _rb.gravityScale = _defaultGravityScale;
                if (_sprite != null) _sprite.color = _baseColor;
                // Bleed off the dash so it doesn't fling the player at full speed.
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x * 0.5f, _rb.linearVelocity.y);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck == null) return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
