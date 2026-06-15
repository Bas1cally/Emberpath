using System;
using Emberpath.Enemy;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Emberpath.Core
{
    /// <summary>
    /// Central run/flow state. Detects when the arena is cleared (all enemies dead)
    /// and handles restart. Foundation for the roguelite run structure (stages,
    /// rewards) — kept minimal for now.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public enum RunState { Playing, Cleared }

        public static GameManager Instance { get; private set; }

        [SerializeField] private KeyCode restartKey = KeyCode.R;

        public RunState State { get; private set; } = RunState.Playing;

        /// <summary>Raised once when the last enemy dies.</summary>
        public event Action ArenaCleared;

        private bool _enemiesSeen;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (Input.GetKeyDown(restartKey)) Restart();
            if (State == RunState.Playing) CheckCleared();
        }

        private void CheckCleared()
        {
            int alive = EnemyController.AliveCount;
            if (alive > 0)
            {
                _enemiesSeen = true;
            }
            else if (_enemiesSeen)
            {
                State = RunState.Cleared;
                Debug.Log("[Emberpath] Arena cleared! Press R to restart.");
                ArenaCleared?.Invoke();
            }
        }

        public void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
