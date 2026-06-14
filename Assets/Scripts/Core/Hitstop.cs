using System.Collections;
using UnityEngine;

namespace Emberpath.Core
{
    /// <summary>
    /// Global helper that briefly freezes time on a successful hit ("hitstop"),
    /// a cheap but very effective way to add weight to attacks. Self-bootstraps,
    /// so any script can call <see cref="Freeze"/> without wiring anything up.
    /// </summary>
    public class Hitstop : MonoBehaviour
    {
        private static Hitstop _instance;
        private Coroutine _routine;
        private float _restoreTimeScale = 1f;

        private static Hitstop Instance
        {
            get
            {
                if (_instance != null) return _instance;

                var go = new GameObject("[Hitstop]");
                _instance = go.AddComponent<Hitstop>();
                DontDestroyOnLoad(go);
                return _instance;
            }
        }

        /// <summary>
        /// Freeze gameplay for <paramref name="duration"/> seconds of real time.
        /// Stacking calls simply restart the freeze with the longest pending time.
        /// </summary>
        public static void Freeze(float duration)
        {
            if (duration <= 0f) return;
            Instance.Run(duration);
        }

        private void Run(float duration)
        {
            // Capture the "live" time scale only when we are not already frozen,
            // so repeated hits don't accidentally store 0 as the restore value.
            if (_routine == null)
            {
                _restoreTimeScale = Time.timeScale;
            }
            else
            {
                StopCoroutine(_routine);
            }

            _routine = StartCoroutine(FreezeRoutine(duration));
        }

        private IEnumerator FreezeRoutine(float duration)
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = _restoreTimeScale;
            _routine = null;
        }
    }
}
