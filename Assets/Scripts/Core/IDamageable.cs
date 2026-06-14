using UnityEngine;

namespace Emberpath.Core
{
    /// <summary>
    /// Describes the payload of a single hit so attackers and victims agree on
    /// the same data. Kept as a plain struct so it is cheap to pass around.
    /// </summary>
    public struct DamageInfo
    {
        public int Amount;
        /// <summary>World-space direction the victim should be pushed in (normalized).</summary>
        public Vector2 KnockbackDirection;
        public float KnockbackForce;
        /// <summary>Source of the hit, e.g. the player. May be null.</summary>
        public GameObject Source;

        public DamageInfo(int amount, Vector2 knockbackDirection, float knockbackForce, GameObject source)
        {
            Amount = amount;
            KnockbackDirection = knockbackDirection.sqrMagnitude > 0.0001f
                ? knockbackDirection.normalized
                : Vector2.zero;
            KnockbackForce = knockbackForce;
            Source = source;
        }
    }

    /// <summary>
    /// Anything that can be hit implements this. Lets the combat code stay
    /// decoupled from concrete enemy types.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Apply a hit. Returns true if the hit "connected" (i.e. the target was
        /// alive and not invulnerable), so the attacker can react with feedback.
        /// </summary>
        bool TakeDamage(in DamageInfo info);
    }
}
