using System.Collections.Generic;
using Emberpath.Core;
using Emberpath.Spells;
using UnityEngine;

namespace Emberpath.Player
{
    /// <summary>
    /// Casts the player's currently selected (equipped) spell. Supports multiple
    /// spell types (projectile, AoE burst) defined as data. Cast automatically on a
    /// perfect dash (<see cref="PlayerParry.PerfectDash"/>) and by a manual key;
    /// cycle the selected spell with <see cref="cycleKey"/> for testing.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class SpellCaster : MonoBehaviour
    {
        [SerializeField]
        private List<SpellDefinition> equipped = new List<SpellDefinition>
        {
            new SpellDefinition { name = "Bolt", type = SpellType.Projectile, damage = 2, cooldown = 0.4f },
            new SpellDefinition { name = "Burst", type = SpellType.Burst, damage = 3, cooldown = 1.1f,
                                  burstRadius = 3.2f, knockback = 11f, color = new Color(1f, 0.7f, 0.3f) },
        };

        [SerializeField] private Sprite projectileSprite;
        [SerializeField] private KeyCode manualCastKey = KeyCode.L;
        [SerializeField] private KeyCode cycleKey = KeyCode.Q;

        public int SelectedIndex { get; private set; }
        public string SelectedName => HasSpell ? equipped[SelectedIndex].name : "-";
        private bool HasSpell => equipped != null && equipped.Count > 0;

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

            if (HasSpell && Input.GetKeyDown(cycleKey))
            {
                SelectedIndex = (SelectedIndex + 1) % equipped.Count;
                Debug.Log($"[Emberpath] Equipped spell: {equipped[SelectedIndex].name}");
            }

            if (Input.GetKeyDown(manualCastKey)) CastEquippedSpell();
        }

        /// <summary>Casts the selected spell if off cooldown.</summary>
        public void CastEquippedSpell()
        {
            if (!HasSpell || _cooldownLeft > 0f) return;

            SpellDefinition spell = equipped[Mathf.Clamp(SelectedIndex, 0, equipped.Count - 1)];
            _cooldownLeft = spell.cooldown;

            int facing = _controller != null ? _controller.FacingDirection : 1;

            switch (spell.type)
            {
                case SpellType.Projectile: CastProjectile(spell, new Vector2(facing, 0f)); break;
                case SpellType.Burst: CastBurst(spell); break;
            }
        }

        private void CastProjectile(SpellDefinition spell, Vector2 dir)
        {
            var go = new GameObject("Spell_Projectile");
            go.transform.position = transform.position + (Vector3)(dir.normalized * 0.6f) + Vector3.up * 0.2f;
            go.transform.localScale = new Vector3(0.45f, 0.45f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = projectileSprite != null ? projectileSprite : PlaceholderSprites.UnitSquare;
            sr.color = projectileSprite != null ? Color.white : spell.color;
            sr.sortingOrder = 15;

            var proj = go.AddComponent<SpellProjectile>();
            proj.Launch(dir, spell.projectileSpeed, spell.damage, spell.knockback, spell.projectileLifetime, ~0, gameObject);
        }

        private void CastBurst(SpellDefinition spell)
        {
            Vector2 center = transform.position;
            var hitOnce = new HashSet<IDamageable>();

            Collider2D[] cols = Physics2D.OverlapCircleAll(center, spell.burstRadius, ~0);
            foreach (Collider2D c in cols)
            {
                if (c.attachedRigidbody != null && c.attachedRigidbody.gameObject == gameObject) continue;

                if (!c.TryGetComponent(out IDamageable target) &&
                    (c.attachedRigidbody == null || !c.attachedRigidbody.TryGetComponent(out target)))
                {
                    continue;
                }
                if (!hitOnce.Add(target)) continue;

                Vector2 d = (Vector2)c.transform.position - center;
                int sx = d.x >= 0f ? 1 : -1;
                target.TakeDamage(new DamageInfo(spell.damage, new Vector2(sx, 0.25f), spell.knockback, gameObject));
            }

            SpawnBurstVisual(center, spell);
        }

        private void SpawnBurstVisual(Vector2 center, SpellDefinition spell)
        {
            var go = new GameObject("Spell_Burst");
            go.transform.position = center;
            float d = spell.burstRadius * 2f;
            go.transform.localScale = new Vector3(d, d, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PlaceholderSprites.UnitSquare;
            sr.color = new Color(spell.color.r, spell.color.g, spell.color.b, 0.4f);
            sr.sortingOrder = 14;

            Destroy(go, 0.12f);
        }
    }
}
