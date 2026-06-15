using Emberpath.Core;
using UnityEngine;

namespace Emberpath.Enemy
{
    /// <summary>
    /// Real enemy AI. Two modes:
    ///  - GroundMelee: patrols, won't walk off ledges or into walls, chases the
    ///    player on sight and does a wind-up melee attack in range.
    ///  - Flyer: hovers, homes in on the player on sight, hurts on contact.
    /// Damage/death go through <see cref="Health"/>; visuals through
    /// <see cref="EnemySpriteAnimator"/>. The target is injected via Configure.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Health))]
    public class EnemyController : MonoBehaviour
    {
        public enum Mode { GroundMelee, Flyer }

        [SerializeField] private Mode mode = Mode.GroundMelee;
        [SerializeField] private bool spriteDefaultFacesRight = true;

        [Header("Movement")]
        [SerializeField] private float patrolSpeed = 2f;
        [SerializeField] private float chaseSpeed = 4.5f;

        [Header("Detection")]
        [SerializeField] private float detectionRange = 8f;
        [SerializeField] private float verticalTolerance = 2.5f;

        [Header("Melee attack")]
        [SerializeField] private float attackRange = 1.3f;
        [SerializeField] private int attackDamage = 1;
        [SerializeField] private float attackKnockback = 11f;
        [SerializeField] private float attackWindup = 0.28f;
        [SerializeField] private float attackCooldown = 1.3f;

        [Header("Contact")]
        [SerializeField] private int contactDamage = 1;
        [SerializeField] private float contactKnockback = 9f;
        [SerializeField] private float contactRange = 1.0f;
        [SerializeField] private float contactCooldown = 0.9f;

        [Header("Ground sensing")]
        [SerializeField] private float wallCheckDistance = 0.6f;
        [SerializeField] private float ledgeCheckAhead = 0.9f;
        [SerializeField] private float ledgeCheckDepth = 1.6f;

        [Header("Reactions")]
        [SerializeField] private float hurtStun = 0.25f;
        [SerializeField] private float deathDespawnDelay = 1.2f;

        private Transform _target;
        private Health _targetHealth;
        private LayerMask _groundMask;

        private Rigidbody2D _rb;
        private Health _health;
        private Collider2D _col;
        private EnemySpriteAnimator _anim;
        private SpriteRenderer _visual;

        private int _facing = -1;
        private float _attackCdLeft;
        private float _contactCdLeft;
        private float _hurtLeft;
        private float _windupLeft;
        private bool _winding;
        private bool _dead;

        public void Configure(Mode enemyMode, Transform target, Health targetHealth, LayerMask groundMask)
        {
            mode = enemyMode;
            _target = target;
            _targetHealth = targetHealth;
            _groundMask = groundMask;
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _health = GetComponent<Health>();
            _col = GetComponent<Collider2D>();
            _anim = GetComponentInChildren<EnemySpriteAnimator>();
            _visual = GetComponentInChildren<SpriteRenderer>();

            _health.Damaged += OnDamaged;
            _health.Died += OnDied;

            if (mode == Mode.Flyer) _rb.gravityScale = 0f;
            FaceTowards(_facing);
        }

        private void OnDestroy()
        {
            if (_health == null) return;
            _health.Damaged -= OnDamaged;
            _health.Died -= OnDied;
        }

        private void Update()
        {
            _attackCdLeft -= Time.deltaTime;
            _contactCdLeft -= Time.deltaTime;
            if (_hurtLeft > 0f) _hurtLeft -= Time.deltaTime;
        }

        private void FixedUpdate()
        {
            if (_dead) return;

            bool detected = TryGetPlayerInfo(out float dx, out float dy, out float dist, out int dirToPlayer);

            if (detected && dist <= contactRange) TryContactDamage();

            // During hurt stun, let physics (knockback + gravity) play out — no AI drive.
            if (_hurtLeft > 0f) return;
            if (_winding) { TickWindup(); return; }

            if (mode == Mode.GroundMelee) GroundBehaviour(detected, dx, dy, dirToPlayer);
            else FlyerBehaviour(detected, dirToPlayer);
        }

        // --- Ground melee ---------------------------------------------------

        private void GroundBehaviour(bool detected, float dx, float dy, int dirToPlayer)
        {
            bool inReach = detected && Mathf.Abs(dy) <= verticalTolerance;

            if (inReach && Mathf.Abs(dx) <= attackRange && _attackCdLeft <= 0f)
            {
                FaceTowards(dirToPlayer);
                StartAttack();
                return;
            }

            int moveDir;
            float speed;
            if (inReach)
            {
                moveDir = dirToPlayer;
                speed = chaseSpeed;
            }
            else
            {
                moveDir = _facing;
                speed = patrolSpeed;
            }

            FaceTowards(moveDir);

            if (IsBlockedAhead())
            {
                if (inReach)
                {
                    Brake();          // chasing but at a ledge/wall: hold position
                    SetMoving(false);
                }
                else
                {
                    _facing = -_facing; // patrolling: turn around
                    FaceTowards(_facing);
                }
                return;
            }

            _rb.linearVelocity = new Vector2(moveDir * speed, _rb.linearVelocity.y);
            SetMoving(true);
        }

        private const float BodyHalfWidth = 0.5f;

        private bool IsBlockedAhead()
        {
            Vector2 origin = _rb.position;
            Vector2 dir = Vector2.right * _facing;

            // Start the wall ray just outside our own body so we never hit ourselves.
            Vector2 wallOrigin = origin + dir * BodyHalfWidth;
            if (Physics2D.Raycast(wallOrigin, dir, wallCheckDistance, _groundMask)) return true;

            // Ledge: probe straight down from a point ahead of the feet.
            Vector2 ahead = origin + dir * ledgeCheckAhead;
            return !Physics2D.Raycast(ahead, Vector2.down, ledgeCheckDepth, _groundMask);
        }

        // --- Flyer ----------------------------------------------------------

        private void FlyerBehaviour(bool detected, int dirToPlayer)
        {
            if (detected && _target != null)
            {
                Vector2 dir = ((Vector2)_target.position - _rb.position).normalized;
                _rb.linearVelocity = dir * chaseSpeed;
                FaceTowards(dirToPlayer);
                SetMoving(true);
            }
            else
            {
                // Gentle hover bob while unaware.
                _rb.linearVelocity = new Vector2(0f, Mathf.Sin(Time.time * 2f) * 0.6f);
                SetMoving(false);
            }
        }

        // --- Attacks --------------------------------------------------------

        private void StartAttack()
        {
            _winding = true;
            _windupLeft = attackWindup;
            _attackCdLeft = attackCooldown;
            Brake();
            SetMoving(false);
            if (_anim != null) _anim.PlayAttack();
        }

        private void TickWindup()
        {
            Brake();
            _windupLeft -= Time.fixedDeltaTime;
            if (_windupLeft > 0f) return;

            _winding = false;

            if (_targetHealth == null || _targetHealth.IsDead) return;
            Vector2 d = (Vector2)_target.position - _rb.position;
            if (Mathf.Abs(d.x) <= attackRange + 0.4f && Mathf.Abs(d.y) <= verticalTolerance)
            {
                int dir = d.x >= 0f ? 1 : -1;
                _targetHealth.TakeDamage(new DamageInfo(attackDamage, new Vector2(dir, 0.2f), attackKnockback, gameObject));
            }
        }

        private void TryContactDamage()
        {
            if (contactDamage <= 0 || _contactCdLeft > 0f) return;
            if (_targetHealth == null || _targetHealth.IsDead) return;

            int dir = _target.position.x >= transform.position.x ? 1 : -1;
            if (_targetHealth.TakeDamage(new DamageInfo(contactDamage, new Vector2(dir, 0.3f), contactKnockback, gameObject)))
                _contactCdLeft = contactCooldown;
        }

        // --- Reactions ------------------------------------------------------

        private void OnDamaged(DamageInfo info)
        {
            if (_dead) return;
            _hurtLeft = hurtStun;
            _winding = false;
            if (_anim != null) _anim.PlayHurt();
        }

        private void OnDied()
        {
            _dead = true;
            _winding = false;
            _rb.linearVelocity = Vector2.zero;
            _rb.bodyType = RigidbodyType2D.Kinematic;
            if (_col != null) _col.enabled = false;
            if (_anim != null) _anim.PlayDeath();
            Destroy(gameObject, deathDespawnDelay);
        }

        // --- Helpers --------------------------------------------------------

        private bool TryGetPlayerInfo(out float dx, out float dy, out float dist, out int dirToPlayer)
        {
            dx = dy = dist = 0f;
            dirToPlayer = _facing;
            if (_target == null || _targetHealth == null || _targetHealth.IsDead) return false;

            Vector2 d = (Vector2)_target.position - _rb.position;
            dx = d.x; dy = d.y; dist = d.magnitude;
            dirToPlayer = dx >= 0f ? 1 : -1;
            return dist <= detectionRange;
        }

        private void Brake()
        {
            _rb.linearVelocity = mode == Mode.Flyer
                ? Vector2.zero
                : new Vector2(0f, _rb.linearVelocity.y);
        }

        private void FaceTowards(int dir)
        {
            if (dir != 0) _facing = dir;
            if (_visual != null)
                _visual.flipX = spriteDefaultFacesRight ? _facing < 0 : _facing > 0;
        }

        private void SetMoving(bool moving)
        {
            if (_anim != null) _anim.SetMoving(moving);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }
    }
}
