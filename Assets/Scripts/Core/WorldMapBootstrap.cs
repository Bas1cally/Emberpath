using UnityEngine;

namespace Emberpath.Core
{
    /// <summary>
    /// Placeholder Super Mario World–style world map, built from code: level nodes in
    /// a path, a token you move with ←/→, and Space/Up to enter the selected level.
    /// You can only move up to your current progress. Real map art/layout comes later;
    /// this proves the map → level → death/complete → map loop.
    /// </summary>
    public class WorldMapBootstrap : MonoBehaviour
    {
        [SerializeField] private int nodeCount = 5;
        [SerializeField] private float spacing = 2.6f;
        [SerializeField] private Color backgroundColor = new Color(0.10f, 0.13f, 0.16f);

        private static readonly Color NodeColor = new Color(0.55f, 0.62f, 0.42f);
        private static readonly Color BossColor = new Color(0.75f, 0.30f, 0.30f);
        private static readonly Color DoneColor = new Color(0.40f, 0.70f, 0.45f);
        private static readonly Color PathColor = new Color(0.30f, 0.30f, 0.34f);
        private static readonly Color TokenColor = new Color(0.95f, 0.55f, 0.20f);

        private Transform _token;
        private int _index;
        private int _maxReachable;

        private void Awake()
        {
            if (RunManager.Instance == null)
                new GameObject("[RunManager]").AddComponent<RunManager>();

            BuildCamera();
            BuildPath();
            BuildNodes();
            BuildToken();

            Debug.Log("[Emberpath] World map. ←/→ select a level, Space/Up to enter.");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                MoveToken(1);
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                MoveToken(-1);

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) ||
                Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Return))
            {
                RunManager.Instance.EnterLevel(_index);
            }
        }

        private void MoveToken(int delta)
        {
            _index = Mathf.Clamp(_index + delta, 0, _maxReachable);
            if (_token != null) _token.position = NodePosition(_index);
        }

        private float StartX => -(nodeCount - 1) * spacing * 0.5f;
        private Vector2 NodePosition(int i) => new Vector2(StartX + i * spacing, 0f);

        private void BuildCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                cam = go.AddComponent<Camera>();
                go.AddComponent<AudioListener>();
            }
            cam.orthographic = true;
            cam.orthographicSize = Mathf.Max(4f, nodeCount * spacing * 0.35f);
            cam.backgroundColor = backgroundColor;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.transform.position = new Vector3(0f, 0f, -10f);
        }

        private void BuildPath()
        {
            for (int i = 0; i < nodeCount - 1; i++)
            {
                Vector2 a = NodePosition(i);
                Vector2 b = NodePosition(i + 1);
                var go = new GameObject($"Path_{i}");
                go.transform.position = (a + b) * 0.5f;
                go.transform.localScale = new Vector3(spacing, 0.18f, 1f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = PlaceholderSprites.UnitSquare;
                sr.color = PathColor;
                sr.sortingOrder = 0;
            }
        }

        private void BuildNodes()
        {
            int reached = RunManager.Instance != null ? RunManager.Instance.CurrentLevel : 0;
            for (int i = 0; i < nodeCount; i++)
            {
                var go = new GameObject($"Node_{i}");
                go.transform.position = NodePosition(i);
                go.transform.localScale = new Vector3(1f, 1f, 1f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = PlaceholderSprites.UnitSquare;
                bool boss = RunManager.Instance != null && RunManager.Instance.IsBoss(i);
                sr.color = i < reached ? DoneColor : (boss ? BossColor : NodeColor);
                sr.sortingOrder = 1;
            }
        }

        private void BuildToken()
        {
            int reached = RunManager.Instance != null ? RunManager.Instance.CurrentLevel : 0;
            _maxReachable = Mathf.Clamp(reached, 0, nodeCount - 1);
            _index = _maxReachable;

            var go = new GameObject("MapToken");
            go.transform.position = NodePosition(_index) + new Vector2(0f, 0.05f);
            go.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PlaceholderSprites.UnitSquare;
            sr.color = TokenColor;
            sr.sortingOrder = 2;
            _token = go.transform;
        }

        private void OnGUI()
        {
            var style = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold };
            GUI.color = Color.white;
            GUI.Label(new Rect(14, 10, 700, 24),
                $"WORLD MAP — selected level {_index} · ←/→ move · Space/Up enter", style);
        }
    }
}
