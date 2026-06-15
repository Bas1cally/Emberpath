using Emberpath.Core;
using Emberpath.Spells;
using UnityEngine;

namespace Emberpath.Player
{
    /// <summary>
    /// Casts the player's currently "equipped" spell. For now the equipped spell is
    /// a single placeholder projectile, fired in the facing direction. It is cast
    /// automatically on a perfect dash (via <see cref="PlayerParry.PerfectDash"/>)
    /// and by a manual key for testing. Later this becomes data-driven/equippable.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class SpellCaster : MonoBehaviour
    {
        [Header("Equipped spell (placeholder projectile)")]
        [SerializeField] private int damage = 2;
        [SerializeField] private float projectileSpeed = 14f;
        [SerializeField] private float knockback = 8f;
        [SerializeField] private float lifetime = 1.6f;
        [SerializeField] private float cooldown = 0.4f;

        [Header("Look")]
        [SerializeField] private Sprite projectileSprite;
        [SerializeField] private Vector2 projectileScale = new Vector2(0.45f, 0.45f);
        [SerializeField] private Color projectileColor = new Color(0.6f, 0.9f, 1f);

        [Header("Input")]
        [SerializeField] private KeyCode manualCastKey = KeyCode.L;

        private PlayerController _controller;
        private PlayerParry _parry;
        private float _cooldownLeft;

        private void Awake()
        {
            _controller = GetComponent<PlayerController>();
            _parry = GetComponent<PlayerParry>();
            if (_parry != null) _parry.PerfectDash += CastEquippedSpell;
        }

        private void OnDestroy()
        {
            if (_parry != null) _parry.PerfectDash -= CastEquippedSpell;
        }

        private void Update()
        {
            if (_cooldownLeft > 0f) _cooldownLeft -= Time.deltaTime;
            if (Input.GetKeyDown(manualCastKey)) CastEquippedSpell();
        }

        /// <summary>Casts the equipped spell if off cooldown.</summary>
        public void CastEquippedSpell()
        {
            if (_cooldownLeft > 0f) return;
            _cooldownLeft = cooldown;

            int facing = _controller != null ? _controller.FacingDirection : 1;
            SpawnProjectile(new Vector2(facing, 0f));
        }

        private void SpawnProjectile(Vector2 dir)
        {
            var go = new GameObject("Spell_Projectile");
            go.transform.position = transform.position + (Vector3)(dir.normalized * 0.6f) + Vector3.up * 0.2f;
            go.transform.localScale = new Vector3(projectileScale.x, projectileScale.y, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = projectileSprite != null ? projectileSprite : PlaceholderSprites.UnitSquare;
            sr.color = projectileSprite != null ? Color.white : projectileColor;
            sr.sortingOrder = 15;

            // Hit everything; the projectile only damages IDamageable and skips its caster.
            var proj = go.AddComponent<SpellProjectile>();
            proj.Launch(dir, projectileSpeed, damage, knockback, lifetime, ~0, gameObject);
        }
    }
}
