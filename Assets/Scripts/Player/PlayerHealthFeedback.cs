using System.Collections;
using Emberpath.Core;
using UnityEngine;

namespace Emberpath.Player
{
    /// <summary>
    /// Player-side reaction to <see cref="Health"/>: a quick hit flash, and on
    /// death a short freeze followed by a respawn at the start position. Keeps the
    /// vertical slice playable while we build out real combat.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class PlayerHealthFeedback : MonoBehaviour
    {
        [SerializeField] private Color hurtFlash = new Color(1f, 0.4f, 0.4f);
        [SerializeField] private float flashTime = 0.1f;
        [SerializeField] private float respawnDelay = 1.0f;

        private Health _health;
        private SpriteRenderer _sprite;
        private PlayerController _controller;
        private PlayerCombat _combat;
        private Rigidbody2D _rb;

        private Color _baseColor = Color.white;
        private Vector3 _spawnPos;
        private Coroutine _flashRoutine;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _sprite = GetComponentInChildren<SpriteRenderer>();
            _controller = GetComponent<PlayerController>();
            _combat = GetComponent<PlayerCombat>();
            _rb = GetComponent<Rigidbody2D>();

            if (_sprite != null) _baseColor = _sprite.color;
            _spawnPos = transform.position;

            _health.Damaged += OnDamaged;
            _health.Died += OnDied;
        }

        private void OnDestroy()
        {
            if (_health == null) return;
            _health.Damaged -= OnDamaged;
            _health.Died -= OnDied;
        }

        private void OnDamaged(DamageInfo info) => Flash();

        private void Flash()
        {
            if (_sprite == null) return;
            if (_flashRoutine != null) StopCoroutine(_flashRoutine);
            _flashRoutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            _sprite.color = hurtFlash;
            yield return new WaitForSecondsRealtime(flashTime);
            _sprite.color = _baseColor;
            _flashRoutine = null;
        }

        private void OnDied() => StartCoroutine(RespawnRoutine());

        private IEnumerator RespawnRoutine()
        {
            // Freeze control and movement during the death beat.
            if (_controller != null) _controller.enabled = false;
            if (_combat != null) _combat.enabled = false;
            if (_rb != null) _rb.linearVelocity = Vector2.zero;
            if (_sprite != null)
                _sprite.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, 0.3f);

            yield return new WaitForSeconds(respawnDelay);

            // Roguelite: death resets the level to the last checkpoint (reloads the
            // scene). Falls back to an in-place respawn if there is no run manager.
            if (RunManager.Instance != null)
            {
                RunManager.Instance.PlayerDied();
                yield break;
            }

            transform.position = _spawnPos;
            if (_rb != null) _rb.linearVelocity = Vector2.zero;
            _health.Revive();

            if (_sprite != null) _sprite.color = _baseColor;
            if (_controller != null) _controller.enabled = true;
            if (_combat != null) _combat.enabled = true;
        }
    }
}
