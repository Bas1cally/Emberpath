using System;
using UnityEngine;

namespace Emberpath.Core
{
    /// <summary>
    /// Shared damageable health for the player and enemies. Handles hit
    /// invulnerability and knockback (on a dynamic Rigidbody2D) and raises events
    /// so visuals/AI can react without coupling to concrete types.
    /// </summary>
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth = 3;
        [Tooltip("Brief invulnerability after a hit, in seconds.")]
        [SerializeField] private float invulnerabilityTime = 0.15f;

        public int Max => maxHealth;
        public int Current { get; private set; }
        public bool IsDead => Current <= 0;

        /// <summary>
        /// Externally forced invulnerability (e.g. the player while dashing). Hits
        /// that arrive while this is true are rejected and raise <see cref="Blocked"/>.
        /// </summary>
        public bool Invulnerable { get; set; }

        /// <summary>Raised on a connecting hit (after health is reduced).</summary>
        public event Action<DamageInfo> Damaged;
        /// <summary>Raised once when health reaches zero.</summary>
        public event Action Died;
        /// <summary>Raised when an incoming hit is rejected because <see cref="Invulnerable"/> was set (a dodge/parry).</summary>
        public event Action<DamageInfo> Blocked;

        private Rigidbody2D _rb;
        private float _invulnerableLeft;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            Current = maxHealth;
        }

        private void Update()
        {
            if (_invulnerableLeft > 0f) _invulnerableLeft -= Time.deltaTime;
        }

        /// <summary>Sets the max and refills. Use when spawning configured enemies.</summary>
        public void Configure(int max)
        {
            maxHealth = Mathf.Max(1, max);
            Current = maxHealth;
        }

        public bool TakeDamage(in DamageInfo info)
        {
            if (IsDead) return false;

            // Actively dodging (e.g. dashing): negate the hit and signal a parry.
            if (Invulnerable)
            {
                Blocked?.Invoke(info);
                return false;
            }

            if (_invulnerableLeft > 0f) return false;

            _invulnerableLeft = invulnerabilityTime;
            Current -= Mathf.Max(0, info.Amount);
            ApplyKnockback(info);
            Damaged?.Invoke(info);

            if (Current <= 0)
            {
                Current = 0;
                Died?.Invoke();
            }
            return true;
        }

        /// <summary>Refills to max (e.g. on respawn).</summary>
        public void Revive()
        {
            Current = maxHealth;
            _invulnerableLeft = 0f;
        }

        private void ApplyKnockback(in DamageInfo info)
        {
            if (info.KnockbackForce <= 0f || _rb == null) return;
            if (_rb.bodyType != RigidbodyType2D.Dynamic) return;

            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0f);
            _rb.AddForce(info.KnockbackDirection * info.KnockbackForce, ForceMode2D.Impulse);
        }
    }
}
