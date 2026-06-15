using UnityEngine;

namespace Emberpath.Player
{
    /// <summary>
    /// Tracks which player abilities are unlocked and applies them. The backbone of
    /// roguelite progression: abilities start locked/unlocked as configured and can
    /// be granted at runtime via <see cref="Unlock"/> (pickups, run rewards, …).
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerAbilities : MonoBehaviour
    {
        public enum Ability { DoubleJump, Dash, Spell }

        [SerializeField] private bool doubleJump;        // off by default (a later unlock)
        [SerializeField] private bool dash = true;
        [SerializeField] private bool spell = true;

        private PlayerController _controller;
        private SpellCaster _caster;

        public bool HasDoubleJump => doubleJump;
        public bool HasDash => dash;
        public bool HasSpell => spell;

        /// <summary>Sets the starting unlock state. Call before the object activates.</summary>
        public void Configure(bool doubleJump, bool dash, bool spell)
        {
            this.doubleJump = doubleJump;
            this.dash = dash;
            this.spell = spell;
        }

        private void Awake()
        {
            _controller = GetComponent<PlayerController>();
            _caster = GetComponent<SpellCaster>();
            Apply();
        }

        public void Unlock(Ability ability)
        {
            switch (ability)
            {
                case Ability.DoubleJump: doubleJump = true; break;
                case Ability.Dash: dash = true; break;
                case Ability.Spell: spell = true; break;
            }
            Apply();
        }

        private void Apply()
        {
            if (_controller != null)
            {
                _controller.SetExtraJumps(doubleJump ? 1 : 0);
                _controller.DashEnabled = dash;
            }
            if (_caster != null) _caster.enabled = spell;
        }
    }
}
