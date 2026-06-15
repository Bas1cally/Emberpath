using Emberpath.Player;
using UnityEngine;

namespace Emberpath.Core
{
    /// <summary>
    /// A level's exit/goal (Mario-style): when the player enters this trigger, the
    /// current level is completed via <see cref="RunManager"/>.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class LevelGoal : MonoBehaviour
    {
        private bool _used;

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_used) return;
            if (other.GetComponentInParent<PlayerController>() == null) return;

            _used = true;
            if (RunManager.Instance != null) RunManager.Instance.CompleteCurrentLevel();
            else Debug.Log("[Emberpath] Level goal reached (no RunManager present).");
        }
    }
}
