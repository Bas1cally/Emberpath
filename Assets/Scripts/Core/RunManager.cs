using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Emberpath.Core
{
    /// <summary>
    /// Roguelite run/progression (Super Mario World style): you advance level by
    /// level from a world map; reaching a level's goal completes it; dying resets to
    /// the last checkpoint. Checkpoints are set after boss stages.
    ///
    /// Persists across scene loads. Real per-level scenes are configured later; until
    /// then it reloads the current scene as a placeholder and logs the intended flow.
    /// </summary>
    public class RunManager : MonoBehaviour
    {
        public static RunManager Instance { get; private set; }

        [Serializable]
        public class LevelInfo
        {
            public string sceneName;
            public bool isBoss;
        }

        [Tooltip("Ordered levels of the run. Empty = single test scene placeholder.")]
        [SerializeField] private List<LevelInfo> levels = new List<LevelInfo>();
        [SerializeField] private KeyCode restartLevelKey = KeyCode.R;

        public int CurrentLevel { get; private set; }
        public int CheckpointLevel { get; private set; }
        public bool CurrentIsBoss => HasLevel(CurrentLevel) && levels[CurrentLevel].isBoss;

        /// <summary>Raised when a level is completed (after reaching its goal).</summary>
        public event Action<int> LevelCompleted;
        /// <summary>Raised when the player dies and the run resets to the checkpoint.</summary>
        public event Action<int> RunReset;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (Input.GetKeyDown(restartLevelKey)) RestartLevel();
        }

        /// <summary>Goal reached: complete the level, set a checkpoint after a boss, advance.</summary>
        public void CompleteCurrentLevel()
        {
            Debug.Log($"[Emberpath] Level {CurrentLevel} complete.");
            LevelCompleted?.Invoke(CurrentLevel);

            if (CurrentIsBoss)
            {
                CheckpointLevel = CurrentLevel + 1;
                Debug.Log($"[Emberpath] Boss cleared — checkpoint set to level {CheckpointLevel}.");
            }

            CurrentLevel++;
            LoadLevel(CurrentLevel);
        }

        /// <summary>Roguelite death: reset progress to the last checkpoint and reload.</summary>
        public void PlayerDied()
        {
            Debug.Log($"[Emberpath] Player died — resetting to checkpoint (level {CheckpointLevel}).");
            CurrentLevel = CheckpointLevel;
            RunReset?.Invoke(CurrentLevel);
            LoadLevel(CurrentLevel);
        }

        /// <summary>Reloads the current level (dev/testing).</summary>
        public void RestartLevel() => LoadLevel(CurrentLevel);

        private void LoadLevel(int index)
        {
            Time.timeScale = 1f;

            if (HasLevel(index) &&
                !string.IsNullOrEmpty(levels[index].sceneName) &&
                Application.CanStreamedLevelBeLoaded(levels[index].sceneName))
            {
                SceneManager.LoadScene(levels[index].sceneName);
            }
            else
            {
                // Placeholder until real level scenes exist: reload the current scene.
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        private bool HasLevel(int i) => levels != null && i >= 0 && i < levels.Count;
    }
}
