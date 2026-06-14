using Emberpath.Enemy;
using Emberpath.Player;
using UnityEngine;

namespace Emberpath.Core
{
    /// <summary>
    /// Builds the whole Movement &amp; Combat test arena from code so the slice is
    /// playable straight from the scene without hand-placing objects or importing
    /// art. Placeholder visuals are flat-coloured squares generated at runtime.
    ///
    /// Put one of these on an empty GameObject in a scene and press Play. The
    /// matching editor tool (Emberpath ▸ Build Test Arena) can also bake the same
    /// layout into the scene as real objects.
    /// </summary>
    public class TestArenaBootstrap : MonoBehaviour
    {
        [Header("Layers")]
        [Tooltip("Name of the layer used for platforms. Must exist in Tags & Layers.")]
        [SerializeField] private string groundLayerName = "Ground";

        [Header("Camera")]
        [SerializeField] private float cameraOrthoSize = 6.5f;
        [SerializeField] private Color backgroundColor = new Color(0.08f, 0.07f, 0.10f);

        [Header("Player Tuning")]
        [Tooltip("Tweak these and press Play to feel the change; values are saved with the scene.")]
        [SerializeField] private PlayerTuning playerTuning = new PlayerTuning();

        private static readonly Color PlayerColor = new Color(0.95f, 0.55f, 0.20f); // ember orange
        private static readonly Color EnemyColor = new Color(0.65f, 0.20f, 0.25f);
        private static readonly Color GroundColor = new Color(0.20f, 0.22f, 0.28f);

        private void Awake()
        {
            int groundLayer = LayerMask.NameToLayer(groundLayerName);
            if (groundLayer < 0)
            {
                Debug.LogWarning($"[Emberpath] Layer '{groundLayerName}' not found; " +
                                 "falling back to Default. Add it under Project Settings ▸ Tags and Layers.");
                groundLayer = 0;
            }

            BuildCamera();
            BuildArena(groundLayer);
            GameObject player = BuildPlayer(groundLayer);
            BuildEnemy(new Vector2(4f, -1.0f));
            BuildEnemy(new Vector2(-5f, 1.6f));

            Debug.Log("[Emberpath] Test arena ready. Controls: A/D or ←/→ move, " +
                      "Space/W jump (double jump in air), Shift/K dash, J/LMB attack.");

            // Keep the bootstrap object around but inert; nothing else to do.
            _ = player;
        }

        private void BuildCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }

            cam.orthographic = true;
            cam.orthographicSize = cameraOrthoSize;
            cam.backgroundColor = backgroundColor;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.transform.position = new Vector3(0f, 1f, -10f);
        }

        private void BuildArena(int groundLayer)
        {
            // A floor plus a few platforms at different heights and gaps.
            CreatePlatform("Floor", new Vector2(0f, -4f), new Vector2(28f, 1f), groundLayer);
            CreatePlatform("Platform_Low", new Vector2(-6f, -1.5f), new Vector2(4f, 0.6f), groundLayer);
            CreatePlatform("Platform_Mid", new Vector2(0.5f, 0.5f), new Vector2(3.5f, 0.6f), groundLayer);
            CreatePlatform("Platform_High", new Vector2(6f, 2.5f), new Vector2(4f, 0.6f), groundLayer);
            CreatePlatform("Platform_Float", new Vector2(-2.5f, 3.2f), new Vector2(2.5f, 0.6f), groundLayer);

            // Walls so the player can't run off the edge of the test space.
            CreatePlatform("Wall_Left", new Vector2(-13.5f, 0f), new Vector2(1f, 9f), groundLayer);
            CreatePlatform("Wall_Right", new Vector2(13.5f, 0f), new Vector2(1f, 9f), groundLayer);
        }

        private GameObject CreatePlatform(string name, Vector2 position, Vector2 size, int layer)
        {
            var go = new GameObject(name);
            go.layer = layer;
            go.transform.position = position;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PlaceholderSprites.UnitSquare;
            sr.color = GroundColor;
            sr.sortingOrder = 0;

            // Collider is authored in local 1x1 space and inherits the transform scale.
            var col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;

            return go;
        }

        private GameObject BuildPlayer(int groundLayer)
        {
            // Build inactive so ConfigureReferences lands before Awake runs.
            var go = new GameObject("Player");
            go.SetActive(false);
            go.transform.position = new Vector2(0f, -2.5f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PlaceholderSprites.UnitSquare;
            sr.color = PlayerColor;
            sr.sortingOrder = 10;
            go.transform.localScale = new Vector3(0.8f, 1.4f, 1f);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 4f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            var col = go.AddComponent<CapsuleCollider2D>();
            col.size = new Vector2(0.9f, 1f);
            col.direction = CapsuleDirection2D.Vertical;

            // Ground check point at the player's feet.
            var groundCheck = new GameObject("GroundCheck").transform;
            groundCheck.SetParent(go.transform, false);
            groundCheck.localPosition = new Vector3(0f, -0.52f, 0f);

            var controller = go.AddComponent<PlayerController>();
            controller.ConfigureReferences(groundCheck, 1 << groundLayer);
            controller.ApplyTuning(playerTuning);

            var combat = go.AddComponent<PlayerCombat>();
            combat.ApplyTuning(playerTuning);

            go.SetActive(true);
            return go;
        }

        private GameObject BuildEnemy(Vector2 position)
        {
            var go = new GameObject("DummyEnemy");
            go.transform.position = position;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PlaceholderSprites.UnitSquare;
            sr.color = EnemyColor;
            sr.sortingOrder = 5;
            go.transform.localScale = new Vector3(1f, 1.4f, 1f);

            // Kinematic: stays solid (blocks the player) but can't be pushed around
            // by walking into it. Knockback is applied via velocity in DummyEnemy.
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.freezeRotation = true;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;

            go.AddComponent<DummyEnemy>();
            return go;
        }
    }
}
