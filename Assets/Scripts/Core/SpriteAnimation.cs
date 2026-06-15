using UnityEngine;

namespace Emberpath.Core
{
    /// <summary>
    /// A reusable list of frames making up one looping or one-shot animation.
    /// Shared by the player and enemy frame animators.
    /// </summary>
    [System.Serializable]
    public class SpriteAnimation
    {
        public Sprite[] frames;
        [Tooltip("Playback speed in frames per second.")]
        public float fps = 10f;
        public bool loop = true;

        public bool HasFrames => frames != null && frames.Length > 0;
        public float Duration => HasFrames && fps > 0f ? frames.Length / fps : 0f;
    }
}
