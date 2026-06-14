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
        [Tooltip("Extra jumps allowed after the grounded jump. 1 = double jump.")]
        [SerializeField] private int extraJumps = 1;
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

        private Rigidbody2D _rb;
        private SpriteRenderer _sprite;
        private float _defaultGravityScale;

        private float _moveInput;
        private bool _isGrounded;
        private int _jumpsRemaining;

        private float _coyoteCounter;
        private float _jumpBufferCounter;

        private float _dashTimeLeft;
        private float _dashCooldownLeft;
        private float _dashDirection;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _sprite = GetComponentInChildren<SpriteRenderer>();
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
            bool wasGrounded = _isGrounded;
            _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

            if (_isGrounded)
            {
                _coyoteCounter = coyoteTime;
                _jumpsRemaining = extraJumps;

                if (!wasGrounded)
                {
                    // Just landed.
                }
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
