using System.Collections;
using Emberpath.Core;
using UnityEngine;

namespace Emberpath.Enemy
{
    /// <summary>
    /// A stationary punching bag used to test hit feedback: knockback, a colour
    /// flash and a short i-frame window. Has no AI of its own. Respawns after
    /// "dying" so the slice stays testable without restarting the scene.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class DummyEnemy : MonoBehaviour, IDamageable
    {
        [Header("Health")]
        [SerializeField] private int maxHealth = 3;
        [Tooltip("Brief invulnerability after a hit so a single swing counts once.")]
        [SerializeField] private float invulnerabilityTime = 0.1f;

        [Header("Feedback")]
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField] private float flashDuration = 0.08f;
        [Tooltip("How fast knockback is damped back to rest.")]
        [SerializeField] private float knockbackRecovery = 8f;

        [Header("Death / Respawn")]
        [Tooltip("Seconds before the dummy pops back to full health. 0 = stay down.")]
        [SerializeField] private float respawnDelay = 1.5f;

        private Rigidbody2D _rb;
        private SpriteRenderer _sprite;
        private Color _baseColor;
        private int _health;
        private float _invulnerableLeft;
        private Coroutine _flashRoutine;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _sprite = GetComponent<SpriteRenderer>();
            _baseColor = _sprite.color;
            _health = maxHealth;
        }

        private void Update()
        {
            _invulnerableLeft -= Time.deltaTime;
        }

        private void FixedUpdate()
        {
            // Ease horizontal knockback back to zero so the dummy settles.
            if (Mathf.Abs(_rb.linearVelocity.x) > 0.01f)
            {
                float x = Mathf.MoveTowards(_rb.linearVelocity.x, 0f, knockbackRecovery * Time.fixedDeltaTime);
                _rb.linearVelocity = new Vector2(x, _rb.linearVelocity.y);
            }
        }

        public bool TakeDamage(in DamageInfo info)
        {
            if (_invulnerableLeft > 0f || _health <= 0) return false;

            _invulnerableLeft = invulnerabilityTime;
            _health -= info.Amount;

            ApplyKnockback(info);
            Flash();

            if (_health <= 0)
            {
                Die();
            }

            return true;
        }

        private void ApplyKnockback(in DamageInfo info)
        {
            if (info.KnockbackForce <= 0f || info.KnockbackDirection == Vector2.zero) return;

            // The dummy is a Kinematic body (so it can't be shoved around just by
            // walking into it), so knockback is driven by setting velocity directly
            // rather than AddForce. Horizontal only, so the floating block returns to
            // rest on its own line.
            float dir = Mathf.Sign(info.KnockbackDirection.x != 0f ? info.KnockbackDirection.x : 1f);
            _rb.linearVelocity = new Vector2(dir * info.KnockbackForce, 0f);
        }

        private void Flash()
        {
            if (_flashRoutine != null) StopCoroutine(_flashRoutine);
            _flashRoutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            _sprite.color = flashColor;
            // Real time so the flash still reads during hitstop.
            yield return new WaitForSecondsRealtime(flashDuration);
            _sprite.color = _baseColor;
            _flashRoutine = null;
        }

        private void Die()
        {
            _sprite.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, 0.25f);
            _rb.linearVelocity = Vector2.zero;

            if (respawnDelay > 0f)
            {
                StartCoroutine(RespawnRoutine());
            }
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(respawnDelay);
            _health = maxHealth;
            if (_flashRoutine != null) { StopCoroutine(_flashRoutine); _flashRoutine = null; }
            _sprite.color = _baseColor;
        }
    }
}
