using UnityEngine;

namespace Emberpath.Spells
{
    public enum SpellType { Projectile, Burst }

    /// <summary>
    /// Data for one equippable spell. Plain serializable class (no ScriptableObject
    /// asset needed yet) so spells can be defined/tuned in the inspector and later
    /// driven by pickups/equipment.
    /// </summary>
    [System.Serializable]
    public class SpellDefinition
    {
        public string name = "Spell";
        public SpellType type = SpellType.Projectile;

        public int damage = 2;
        public float cooldown = 0.5f;
        public float knockback = 8f;
        public Color color = new Color(0.6f, 0.9f, 1f);

        [Header("Projectile")]
        public float projectileSpeed = 14f;
        public float projectileLifetime = 1.6f;

        [Header("Burst (AoE around the caster)")]
        public float burstRadius = 3f;
    }
}
