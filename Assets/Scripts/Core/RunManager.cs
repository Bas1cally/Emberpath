using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Emberpath.Core
{
    /// <summary>
    /// Roguelite run/progression, Super Mario World style: a world map is the hub;
    /// you enter a level node, play the level, and on completing it (reaching the
    /// goal) you return to the map with the next node unlocked. Dying resets to the
    /// last checkpoint; checkpoints are set after boss stages.
    ///
    /// Persists across scene loads. Per-level scenes are configured in
    /// <see cref="levels"/>; until they exist it falls back to the default test level.
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

        [SerializeField] private string worldMapScene = "WorldMap";
        [SerializeField] private string defaultLevelScene = "TestArena";
        [Tooltip("Ordered levels of the run. Empty = single placeholder test level.")]
        [SerializeField] private List<LevelInfo> levels = new List<LevelInfo>();
        [SerializeField] private KeyCode restartLevelKey = KeyCode.R;

        public int CurrentLevel { get; private set; }
        public int CheckpointLevel { get; private set; }
        public int LevelCount => Mathf.Max(1, levels.Count);
        public bool IsBoss(int index) => HasLevel(index) && levels[index].isBoss;

        /// <summary>
        /// True only while playing a level launched from the world map. When false
        /// (e.g. play-testing a level scene directly) death/goal don't jump to the map.
        /// </summary>
        public bool RunActive { get; private set; }

        public event Action<int> LevelCompleted;
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

        /// <summary>Enter a level from the world map.</summary>
        public void EnterLevel(int index)
        {
            CurrentLevel = Mathf.Max(0, index);
            RunActive = true;
            Debug.Log($"[Emberpath] Entering level {CurrentLevel}.");
            LoadScene(LevelSceneName(CurrentLevel));
        }

        /// <summary>Goal reached: checkpoint after a boss, advance, return to the map.</summary>
        public void CompleteCurrentLevel()
        {
            Debug.Log($"[Emberpath] Level {CurrentLevel} complete.");
            LevelCompleted?.Invoke(CurrentLevel);

            if (IsBoss(CurrentLevel))
            {
                CheckpointLevel = CurrentLevel + 1;
                Debug.Log($"[Emberpath] Boss cleared — checkpoint set to level {CheckpointLevel}.");
            }

            CurrentLevel++;
            ReturnToMap();
        }

        /// <summary>Roguelite death: reset to the last checkpoint and return to the map.</summary>
        public void PlayerDied()
        {
            Debug.Log($"[Emberpath] Player died — resetting to checkpoint (level {CheckpointLevel}).");
            CurrentLevel = CheckpointLevel;
            RunReset?.Invoke(CurrentLevel);
            ReturnToMap();
        }

        public void ReturnToMap()
        {
            RunActive = false;
            LoadScene(worldMapScene);
        }
        public void RestartLevel() => LoadScene(LevelSceneName(CurrentLevel));

        private string LevelSceneName(int index)
        {
            return HasLevel(index) && !string.IsNullOrEmpty(levels[index].sceneName)
                ? levels[index].sceneName
                : defaultLevelScene;
        }

        private void LoadScene(string sceneName)
        {
            Time.timeScale = 1f;
            if (!string.IsNullOrEmpty(sceneName) && Application.CanStreamedLevelBeLoaded(sceneName))
                SceneManager.LoadScene(sceneName);
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // fallback: reload current
        }

        private bool HasLevel(int i) => levels != null && i >= 0 && i < levels.Count;
    }
}
