using UnityEngine;

namespace Emberpath.Core
{
    /// <summary>
    /// Generates flat placeholder sprites at runtime so the vertical slice needs
    /// no imported art. A single 1x1 white square is created once and tinted per
    /// object via <see cref="SpriteRenderer.color"/>.
    /// </summary>
    public static class PlaceholderSprites
    {
        private static Sprite _unitSquare;

        /// <summary>A 1x1 world-unit white square with a centred pivot.</summary>
        public static Sprite UnitSquare
        {
            get
            {
                if (_unitSquare != null) return _unitSquare;

                const int size = 4;
                var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
                {
                    name = "Placeholder_White",
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp
                };

                var pixels = new Color32[size * size];
                for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(255, 255, 255, 255);
                tex.SetPixels32(pixels);
                tex.Apply();

                // pixelsPerUnit == width => the sprite spans exactly one world unit.
                _unitSquare = Sprite.Create(
                    tex,
                    new Rect(0, 0, size, size),
                    new Vector2(0.5f, 0.5f),
                    size);
                _unitSquare.name = "UnitSquare";
                return _unitSquare;
            }
        }
    }
}
