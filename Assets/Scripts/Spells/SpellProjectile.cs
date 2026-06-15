using Emberpath.Core;
using UnityEngine;

namespace Emberpath.Spells
{
    /// <summary>
    /// A simple homing-free projectile: flies in a direction, damages the first
    /// <see cref="IDamageable"/> it overlaps (other than its caster), and expires
    /// after a lifetime. Foundation for the data-driven spell system.
    /// </summary>
    public class SpellProjectile : MonoBehaviour
    {
        private Vector2 _direction;
        private float _speed;
        private int _damage;
        private float _knockback;
        private float _life;
        private LayerMask _hitMask;
        private GameObject _source;
        private Health _onlyDamage;
        private bool _launched;

        public void Launch(Vector2 direction, float speed, int damage, float knockback,
                           float lifetime, LayerMask hitMask, GameObject source, Health onlyDamage = null)
        {
            _direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
            _speed = speed;
            _damage = damage;
            _knockback = knockback;
            _life = lifetime;
            _hitMask = hitMask;
            _source = source;
            _onlyDamage = onlyDamage; // when set, only this target is damaged (no friendly fire)
            _launched = true;
        }

        private void Update()
        {
            if (!_launched) return;

            transform.position += (Vector3)(_direction * (_speed * Time.deltaTime));

            _life -= Time.deltaTime;
            if (_life <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.3f, _hitMask);
            foreach (Collider2D c in hits)
            {
                if (IsSource(c)) continue;
                if (TryDamage(c))
                {
                    Destroy(gameObject);
                    return;
                }
            }
        }

        private bool IsSource(Collider2D c)
        {
            if (_source == null) return false;
            if (c.gameObject == _source) return true;
            return c.attachedRigidbody != null && c.attachedRigidbody.gameObject == _source;
        }

        private bool TryDamage(Collider2D c)
        {
            if (!c.TryGetComponent(out IDamageable target) &&
                (c.attachedRigidbody == null || !c.attachedRigidbody.TryGetComponent(out target)))
            {
                return false;
            }

            // Restricted projectiles (e.g. enemy shots) only damage their intended target.
            if (_onlyDamage != null && !ReferenceEquals(target, _onlyDamage)) return false;

            int dir = _direction.x >= 0f ? 1 : -1;
            return target.TakeDamage(new DamageInfo(_damage, new Vector2(dir, 0.1f), _knockback, _source));
        }
    }
}
