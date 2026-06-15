using Emberpath.Enemy;
using UnityEngine;

namespace Emberpath.Core
{
    /// <summary>
    /// Minimal on-screen debug HUD (IMGUI) for play-testing: player health, live
    /// enemy count and the control list. No Canvas/font setup needed. Replace with a
    /// real uGUI HUD later.
    /// </summary>
    public class DebugHud : MonoBehaviour
    {
        private Health _player;
        private GUIStyle _style;

        public void Configure(Health player) => _player = player;

        private void OnGUI()
        {
            _style ??= new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold };

            const int x = 14;
            int y = 10;

            GUI.color = Color.white;
            if (_player != null)
            {
                GUI.color = _player.Current <= 1 ? Color.red : Color.white;
                GUI.Label(new Rect(x, y, 400, 24), $"HP: {_player.Current}/{_player.Max}", _style);
                y += 22;
            }

            GUI.color = Color.white;
            GUI.Label(new Rect(x, y, 400, 24), $"Enemies: {EnemyController.AliveCount}", _style);
            y += 22;
            GUI.Label(new Rect(x, y, 800, 24),
                "Move A/D · Jump Space · Dash Shift (dodge/parry) · Attack J · Spell L · Pause Esc · Restart R", _style);
            y += 22;

            if (GameManager.Instance != null && GameManager.Instance.State == GameManager.RunState.Cleared)
            {
                GUI.color = new Color(0.5f, 1f, 0.5f);
                GUI.Label(new Rect(x, y, 500, 30), "ARENA CLEARED — press R", _style);
            }
        }
    }
}
