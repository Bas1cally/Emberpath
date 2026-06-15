using System;
using Emberpath.Core;
using UnityEngine;

namespace Emberpath.Player
{
    /// <summary>All player animation states. Drop your frame sequences into each.</summary>
    [Serializable]
    public class PlayerAnimationSet
    {
        public SpriteAnimation idle = new SpriteAnimation();
        public SpriteAnimation run = new SpriteAnimation();
        public SpriteAnimation jump = new SpriteAnimation { loop = false };
        public SpriteAnimation fall = new SpriteAnimation { loop = false };
        public SpriteAnimation dash = new SpriteAnimation { loop = false };
        public SpriteAnimation attack = new SpriteAnimation { loop = false };
        public SpriteAnimation hurt = new SpriteAnimation { loop = false };

        public bool AnyAssigned =>
            idle.HasFrames || run.HasFrames || jump.HasFrames || fall.HasFrames ||
            dash.HasFrames || attack.HasFrames || hurt.HasFrames;
    }

    /// <summary>
    /// Plays the right sprite animation based on the player's current movement and
    /// combat state — no Animator Controller / graph required. Frame lists are
    /// supplied via <see cref="Configure"/> (e.g. by TestArenaBootstrap) and edited
    /// in the inspector.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerSpriteAnimator : MonoBehaviour
    {
        [SerializeField] private PlayerAnimationSet animations = new PlayerAnimationSet();
        [Tooltip("Horizontal speed above which the run animation plays.")]
        [SerializeField] private float runSpeedThreshold = 0.6f;

        /// <summary>Length of the attack animation in seconds (0 if none assigned).</summary>
        public float AttackDuration => animations != null ? animations.attack.Duration : 0f;

        private SpriteRenderer _sr;
        private PlayerController _controller;
        private PlayerCombat _combat;
        private bool _hooked;

        private SpriteAnimation _current;
        private float _frameTimer;
        private int _frameIndex;

        // One-shot animation (attack/hurt) that plays to completion before
        // movement animations resume.
        private SpriteAnimation _oneShot;
        private float _oneShotTimeLeft;

        /// <summary>Wires up data and references before the object is activated.</summary>
        public void Configure(PlayerAnimationSet set, PlayerController controller, PlayerCombat combat)
        {
            if (set != null) animations = set;
            _controller = controller;
            _combat = combat;
            Hook();
        }

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            if (_controller == null) _controller = GetComponentInParent<PlayerController>();
            if (_combat == null) _combat = GetComponentInParent<PlayerCombat>();
            Hook();
        }

        private void Hook()
        {
            if (_hooked || _combat == null) return;
            _combat.Attacked += OnAttacked;
            _hooked = true;
        }

        private void OnDestroy()
        {
            if (_combat != null) _combat.Attacked -= OnAttacked;
        }

        private void OnAttacked() => StartOneShot(animations.attack);

        /// <summary>Plays the hurt animation once (for when the player takes damage later).</summary>
        public void PlayHurt() => StartOneShot(animations.hurt);

        private void StartOneShot(SpriteAnimation anim)
        {
            if (anim == null || !anim.HasFrames) return;
            _oneShot = anim;
            _oneShotTimeLeft = Mathf.Max(anim.Duration, 0.05f);
        }

        private void Update()
        {
            if (_sr == null) return;

            SpriteAnimation target = SelectAnimation();
            if (target == null || !target.HasFrames) return;

            if (target != _current)
            {
                _current = target;
                _frameIndex = 0;
                _frameTimer = 0f;
                _sr.sprite = target.frames[0];
            }

            Advance();
        }

        private SpriteAnimation SelectAnimation()
        {
            if (_oneShot != null)
            {
                _oneShotTimeLeft -= Time.deltaTime;
                if (_oneShotTimeLeft > 0f) return _oneShot;
                _oneShot = null;
            }

            if (_controller == null) return animations.idle;

            if (_controller.IsDashing && animations.dash.HasFrames) return animations.dash;

            Vector2 v = _controller.Velocity;
            if (!_controller.IsGrounded)
            {
                if (v.y > 0.1f && animations.jump.HasFrames) return animations.jump;
                if (animations.fall.HasFrames) return animations.fall;
                if (animations.jump.HasFrames) return animations.jump;
            }

            if (Mathf.Abs(v.x) > runSpeedThreshold && animations.run.HasFrames) return animations.run;

            return animations.idle.HasFrames ? animations.idle : _current;
        }

        private void Advance()
        {
            if (_current.frames.Length <= 1) return;

            float secondsPerFrame = _current.fps > 0f ? 1f / _current.fps : 0.1f;
            _frameTimer += Time.deltaTime;

            while (_frameTimer >= secondsPerFrame)
            {
                _frameTimer -= secondsPerFrame;
                _frameIndex++;
                if (_frameIndex >= _current.frames.Length)
                    _frameIndex = _current.loop ? 0 : _current.frames.Length - 1;
                _sr.sprite = _current.frames[_frameIndex];
            }
        }
    }
}
