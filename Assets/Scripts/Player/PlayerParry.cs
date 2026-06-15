using System;
using Emberpath.Core;
using UnityEngine;

namespace Emberpath.Player
{
    /// <summary>
    /// Turns the dash into a dodge/parry: the player is invulnerable for the (short)
    /// dash duration. If an attack is negated during a tight window at the start of
    /// the dash, it counts as a "perfect dash" and raises <see cref="PerfectDash"/> —
    /// the future spell system will hook this to auto-cast the equipped spell.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class PlayerParry : MonoBehaviour
    {
        [Tooltip("A dodge within this many seconds of the dash starting is a 'perfect dash'. " +
                 "Keep it tight; <= dash duration.")]
        [SerializeField] private float perfectWindow = 0.18f;

        [Tooltip("Brief tint shown on a perfect dash, as placeholder feedback.")]
        [SerializeField] private Color perfectFlash = new Color(0.6f, 0.95f, 1f);

        /// <summary>Raised on a perfectly-timed dash dodge. Hook the equipped spell here later.</summary>
        public event Action PerfectDash;

        private PlayerController _controller;
        private Health _health;

        private bool _wasDashing;
        private float _dashStartTime = -999f;

        private void Awake()
        {
            _controller = GetComponent<PlayerController>();
            _health = GetComponent<Health>();
            _health.Blocked += OnBlocked;
        }

        private void OnDestroy()
        {
            if (_health != null) _health.Blocked -= OnBlocked;
        }

        private void Update()
        {
            bool dashing = _controller != null && _controller.IsDashing;
            if (dashing && !_wasDashing) _dashStartTime = Time.time; // dash just began
            _wasDashing = dashing;

            // Dash i-frames = the dodge window (kept short = not too generous).
            if (_health != null) _health.Invulnerable = dashing;
        }

        private void OnBlocked(DamageInfo info)
        {
            bool perfect = Time.time - _dashStartTime <= perfectWindow;
            if (!perfect) return;

            PerfectDash?.Invoke();
            // TODO: cast the equipped spell here once the spell system exists.
            Debug.Log("[Emberpath] Perfect dash! (equipped spell would trigger here)");
        }
    }
}
