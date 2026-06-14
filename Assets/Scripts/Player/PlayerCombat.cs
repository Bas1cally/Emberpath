using System.Collections.Generic;
using Emberpath.Core;
using UnityEngine;

namespace Emberpath.Player
{
    /// <summary>
    /// Melee attack for the vertical slice. On input it opens a rectangular
    /// hitbox in front of the player, applies damage + knockback to anything
    /// <see cref="IDamageable"/> it overlaps, and adds hitstop on a clean hit.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Attack")]
        [SerializeField] private int damage = 1;
        [SerializeField] private float attackCooldown = 0.35f;
        [SerializeField] private float knockbackForce = 12f;

        [Header("Hitbox")]
        [Tooltip("Local offset of the hitbox centre from the player (X is mirrored by facing).")]
        [SerializeField] private Vector2 hitboxOffset = new Vector2(0.9f, 0f);
        [SerializeField] private Vector2 hitboxSize = new Vector2(1.4f, 1.2f);
        [SerializeField] private LayerMask hittableLayers = ~0;

        [Header("Feel")]
        [Tooltip("Seconds of time-freeze applied on a connecting hit.")]
        [SerializeField] private float hitstopDuration = 0.08f;
        [Tooltip("How long the debug hitbox gizmo stays visible after an attack.")]
        [SerializeField] private float gizmoFlashTime = 0.1f;

        private PlayerController _controller;
        private float _cooldownLeft;
        private float _gizmoTimer;

        // Reused so a single swing can't damage the same target twice.
        private readonly HashSet<IDamageable> _hitThisSwing = new HashSet<IDamageable>();

        private void Awake()
        {
            _controller = GetComponent<PlayerController>();
        }

        /// <summary>Overrides the combat feel values from a shared tuning object.</summary>
        public void ApplyTuning(PlayerTuning t)
        {
            if (t == null) return;
            damage = t.damage;
            attackCooldown = t.attackCooldown;
            knockbackForce = t.knockbackForce;
            hitstopDuration = t.hitstopDuration;
        }

        private void Update()
        {
            _cooldownLeft -= Time.deltaTime;
            _gizmoTimer -= Time.deltaTime;

            bool attackPressed = Input.GetKeyDown(KeyCode.J)
                                 || Input.GetMouseButtonDown(0)
                                 || Input.GetKeyDown(KeyCode.JoystickButton0);

            if (attackPressed && _cooldownLeft <= 0f)
            {
                Attack();
            }
        }

        private void Attack()
        {
            _cooldownLeft = attackCooldown;
            _gizmoTimer = gizmoFlashTime;
            _hitThisSwing.Clear();

            Vector2 center = GetHitboxCenter();
            Collider2D[] hits = Physics2D.OverlapBoxAll(center, hitboxSize, 0f, hittableLayers);

            bool connected = false;
            foreach (Collider2D col in hits)
            {
                if (col.attachedRigidbody != null && col.attachedRigidbody.gameObject == gameObject)
                    continue; // never hit ourselves

                if (!col.TryGetComponent(out IDamageable target) &&
                    (col.attachedRigidbody == null || !col.attachedRigidbody.TryGetComponent(out target)))
                {
                    continue;
                }

                if (!_hitThisSwing.Add(target)) continue;

                var info = new DamageInfo(
                    damage,
                    new Vector2(_controller.FacingDirection, 0.25f),
                    knockbackForce,
                    gameObject);

                if (target.TakeDamage(info))
                {
                    connected = true;
                }
            }

            if (connected)
            {
                Hitstop.Freeze(hitstopDuration);
            }
        }

        private Vector2 GetHitboxCenter()
        {
            float dir = _controller != null ? _controller.FacingDirection : 1;
            return (Vector2)transform.position + new Vector2(hitboxOffset.x * dir, hitboxOffset.y);
        }

        private void OnDrawGizmosSelected()
        {
            DrawHitboxGizmo(new Color(1f, 0.4f, 0.1f, 0.35f));
        }

        private void OnDrawGizmos()
        {
            // Briefly show the hitbox right after an attack, even when not selected,
            // so it is easy to eyeball reach while play-testing.
            if (Application.isPlaying && _gizmoTimer > 0f)
            {
                DrawHitboxGizmo(new Color(1f, 0.2f, 0.05f, 0.55f));
            }
        }

        private void DrawHitboxGizmo(Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawCube(GetHitboxCenter(), hitboxSize);
        }
    }
}
