using UnityEngine;

namespace Emberpath.Core
{
    /// <summary>
    /// One place to expose the player's "feel" numbers so they can be tweaked in
    /// the inspector and persist (the <c>TestArenaBootstrap</c> object holds an
    /// instance and applies it to the player it spawns). Defaults mirror the
    /// PlayerController / PlayerCombat defaults, so behaviour is unchanged until
    /// you start tuning.
    /// </summary>
    [System.Serializable]
    public class PlayerTuning
    {
        [Header("Run")]
        public float moveSpeed = 8f;

        [Header("Jump")]
        public float jumpForce = 15f;
        [Tooltip("Extra mid-air jumps. 1 = double jump.")]
        public int extraJumps = 1;
        [Tooltip("Higher = snappier, less floaty fall.")]
        public float fallGravityMultiplier = 2.0f;

        [Header("Dash")]
        public float dashSpeed = 22f;
        public float dashDuration = 0.15f;
        public float dashCooldown = 0.5f;

        [Header("Combat")]
        public int damage = 1;
        public float attackCooldown = 0.35f;
        public float knockbackForce = 12f;
        [Tooltip("Seconds of time-freeze on a connecting hit. More = beefier hits.")]
        public float hitstopDuration = 0.08f;
    }
}
