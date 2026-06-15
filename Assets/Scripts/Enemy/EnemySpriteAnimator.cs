using Emberpath.Core;
using UnityEngine;

namespace Emberpath.Enemy
{
    /// <summary>Enemy animation states. Drop frame sequences into each.</summary>
    [System.Serializable]
    public class EnemyAnimationSet
    {
        public SpriteAnimation idle = new SpriteAnimation();
        public SpriteAnimation move = new SpriteAnimation();
        public SpriteAnimation attack = new SpriteAnimation { loop = false };
        public SpriteAnimation hurt = new SpriteAnimation { loop = false };
        public SpriteAnimation death = new SpriteAnimation { loop = false };

        public bool AnyAssigned =>
            idle.HasFrames || move.HasFrames || attack.HasFrames || hurt.HasFrames || death.HasFrames;
    }

    /// <summary>
    /// Plays an enemy's idle loop and one-shot hurt/death animations on command —
    /// driven by <see cref="DummyEnemy"/>. No Animator Controller required.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class EnemySpriteAnimator : MonoBehaviour
    {
        [SerializeField] private EnemyAnimationSet animations = new EnemyAnimationSet();

        private SpriteRenderer _sr;
        private SpriteAnimation _current;
        private float _timer;
        private int _index;

        private SpriteAnimation _oneShot;
        private float _oneShotTimeLeft;
        private bool _dead;
        private bool _moving;

        public void Configure(EnemyAnimationSet set)
        {
            if (set != null) animations = set;
        }

        private void Awake() => _sr = GetComponent<SpriteRenderer>();

        /// <summary>Switches the looping base animation between idle and move.</summary>
        public void SetMoving(bool moving) => _moving = moving;

        public void PlayAttack()
        {
            if (_dead || !animations.attack.HasFrames) return;
            _oneShot = animations.attack;
            _oneShotTimeLeft = Mathf.Max(animations.attack.Duration, 0.05f);
        }

        public void PlayHurt()
        {
            if (_dead || !animations.hurt.HasFrames) return;
            _oneShot = animations.hurt;
            _oneShotTimeLeft = Mathf.Max(animations.hurt.Duration, 0.05f);
        }

        public void PlayDeath()
        {
            _dead = true;
            _oneShot = null;
            if (animations.death.HasFrames) SetCurrent(animations.death);
        }

        public void ResetToIdle()
        {
            _dead = false;
            _oneShot = null;
            SetCurrent(animations.idle);
        }

        private void Update()
        {
            if (_sr == null) return;

            SpriteAnimation target = SelectAnimation();
            if (target == null || !target.HasFrames) return;

            if (target != _current) SetCurrent(target);
            Advance();
        }

        private SpriteAnimation SelectAnimation()
        {
            // While dead, hold on the death animation (last frame) until reset.
            if (_dead) return animations.death.HasFrames ? animations.death : _current;

            if (_oneShot != null)
            {
                _oneShotTimeLeft -= Time.deltaTime;
                if (_oneShotTimeLeft > 0f) return _oneShot;
                _oneShot = null;
            }

            if (_moving && animations.move.HasFrames) return animations.move;
            return animations.idle.HasFrames ? animations.idle : _current;
        }

        private void SetCurrent(SpriteAnimation anim)
        {
            _current = anim;
            _index = 0;
            _timer = 0f;
            if (anim != null && anim.HasFrames) _sr.sprite = anim.frames[0];
        }

        private void Advance()
        {
            if (_current == null || _current.frames.Length <= 1) return;

            float secondsPerFrame = _current.fps > 0f ? 1f / _current.fps : 0.1f;
            _timer += Time.deltaTime;

            while (_timer >= secondsPerFrame)
            {
                _timer -= secondsPerFrame;
                _index++;
                if (_index >= _current.frames.Length)
                    _index = _current.loop ? 0 : _current.frames.Length - 1;
                _sr.sprite = _current.frames[_index];
            }
        }
    }
}
