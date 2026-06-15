using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Emberpath.UI
{
    /// <summary>
    /// Placeholder pause/equipment menu, built from code (uGUI). Opens with Esc and
    /// pauses the game. Layout: a framed panel with equipment on the left
    /// (weapon, armor, two rings), spells on the right, and the idle character in
    /// the centre. Deliberately light on RPG slots — easy to restyle later.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private KeyCode toggleKey = KeyCode.Escape;

        private static readonly Color Dim = new Color(0f, 0f, 0f, 0.6f);
        private static readonly Color FrameColor = new Color(0.62f, 0.49f, 0.27f);   // brassy border
        private static readonly Color PanelColor = new Color(0.10f, 0.09f, 0.12f, 0.98f);
        private static readonly Color SlotColor = new Color(0.62f, 0.49f, 0.27f);
        private static readonly Color SlotInner = new Color(0.17f, 0.15f, 0.18f);
        private static readonly Color TextColor = new Color(0.92f, 0.88f, 0.78f);

        private GameObject _root;
        private Image _characterImage;
        private Font _font;
        private bool _open;

        private void Awake()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null) _font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            EnsureEventSystem();
            Build();
            SetOpen(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey)) SetOpen(!_open);
        }

        /// <summary>Shows the idle character art in the centre (placeholder otherwise).</summary>
        public void SetCharacterSprite(Sprite sprite)
        {
            if (_characterImage == null || sprite == null) return;
            _characterImage.sprite = sprite;
            _characterImage.color = Color.white;
            _characterImage.preserveAspect = true;
        }

        private void SetOpen(bool open)
        {
            _open = open;
            if (_root != null) _root.SetActive(open);
            Time.timeScale = open ? 0f : 1f;
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(es);
        }

        private void Build()
        {
            // Canvas
            _root = new GameObject("PauseCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _root.transform.SetParent(transform, false);
            var canvas = _root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            var scaler = _root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            // Full-screen dim
            Image dim = Panel(_root.transform, "Dim", Vector2.zero, Vector2.zero, Dim);
            Stretch(dim.rectTransform);

            // Framed panel (border + inner)
            Image frame = Panel(_root.transform, "Frame", Vector2.zero, new Vector2(1180f, 700f), FrameColor);
            Image panel = Panel(frame.transform, "Panel", Vector2.zero, new Vector2(1148f, 668f), PanelColor);

            Label(panel.transform, "PAUSE", new Vector2(0f, 300f), new Vector2(400f, 50f), 40, TextAnchor.MiddleCenter, TextColor);

            // Left column — equipment
            string[] equip = { "Weapon", "Armor", "Ring I", "Ring II" };
            ColumnHeader(panel.transform, "EQUIPMENT", -400f, 230f);
            BuildColumn(panel.transform, equip, -400f, 150f, -120f);

            // Right column — spells
            string[] spells = { "Spell I", "Spell II", "Spell III" };
            ColumnHeader(panel.transform, "SPELLS", 400f, 230f);
            BuildColumn(panel.transform, spells, 400f, 120f, -120f);

            // Centre — character
            Image charBox = Panel(panel.transform, "Character", new Vector2(0f, -10f), new Vector2(320f, 460f), SlotColor);
            Image charInner = Panel(charBox.transform, "Inner", Vector2.zero, new Vector2(304f, 444f), SlotInner);
            _characterImage = Panel(charInner.transform, "CharSprite", Vector2.zero, new Vector2(260f, 400f), new Color(1f, 1f, 1f, 0.25f));
            Label(panel.transform, "CHARACTER", new Vector2(0f, -260f), new Vector2(320f, 28f), 20, TextAnchor.MiddleCenter, TextColor);

            // Resume button
            BuildResumeButton(panel.transform);
        }

        private void ColumnHeader(Transform parent, string text, float x, float y)
        {
            Label(parent, text, new Vector2(x, y), new Vector2(280f, 28f), 22, TextAnchor.MiddleCenter, FrameColor);
        }

        private void BuildColumn(Transform parent, string[] labels, float x, float startY, float spacing)
        {
            const float box = 110f;
            for (int i = 0; i < labels.Length; i++)
            {
                float y = startY + i * spacing;
                Image slot = Panel(parent, "Slot_" + labels[i], new Vector2(x, y), new Vector2(box, box), SlotColor);
                Panel(slot.transform, "Inner", Vector2.zero, new Vector2(box - 8f, box - 8f), SlotInner);
                Label(parent, labels[i], new Vector2(x, y - (box / 2f + 18f)), new Vector2(220f, 24f), 18, TextAnchor.MiddleCenter, TextColor);
            }
        }

        private void BuildResumeButton(Transform parent)
        {
            Image btn = Panel(parent, "ResumeButton", new Vector2(0f, -300f), new Vector2(260f, 56f), SlotColor);
            Panel(btn.transform, "Inner", Vector2.zero, new Vector2(252f, 48f), SlotInner);
            Label(btn.transform, "RESUME  (Esc)", Vector2.zero, new Vector2(252f, 30f), 22, TextAnchor.MiddleCenter, TextColor);

            var button = btn.gameObject.AddComponent<Button>();
            button.targetGraphic = btn;
            button.onClick.AddListener(() => SetOpen(false));
        }

        // --- uGUI builders ---------------------------------------------------

        private static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static Image Panel(Transform parent, string name, Vector2 pos, Vector2 size, Color color)
        {
            RectTransform rt = NewRect(name, parent);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            return img;
        }

        private Text Label(Transform parent, string text, Vector2 pos, Vector2 size, int fontSize, TextAnchor anchor, Color color)
        {
            RectTransform rt = NewRect("Label", parent);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            var t = rt.gameObject.AddComponent<Text>();
            t.text = text;
            t.font = _font;
            t.fontSize = fontSize;
            t.alignment = anchor;
            t.color = color;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
