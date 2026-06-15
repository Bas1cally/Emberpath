using UnityEngine;

namespace Emberpath.Core
{
    /// <summary>
    /// Smoothly follows a target (the player). Optional world bounds keep the view
    /// inside the level. Runs in LateUpdate so it tracks after movement.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float smoothTime = 0.15f;
        [SerializeField] private Vector2 offset = new Vector2(0f, 1f);

        [Header("Bounds (optional)")]
        [SerializeField] private bool useBounds;
        [SerializeField] private Vector2 minBounds = new Vector2(-15f, -6f);
        [SerializeField] private Vector2 maxBounds = new Vector2(15f, 9f);

        private Camera _cam;
        private Vector3 _velocity;

        public void SetTarget(Transform t) => target = t;

        private void Awake() => _cam = GetComponent<Camera>();

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desired = new Vector3(
                target.position.x + offset.x,
                target.position.y + offset.y,
                transform.position.z);

            if (useBounds && _cam != null && _cam.orthographic)
            {
                float halfHeight = _cam.orthographicSize;
                float halfWidth = halfHeight * _cam.aspect;
                if (maxBounds.x - minBounds.x > 2f * halfWidth)
                    desired.x = Mathf.Clamp(desired.x, minBounds.x + halfWidth, maxBounds.x - halfWidth);
                if (maxBounds.y - minBounds.y > 2f * halfHeight)
                    desired.y = Mathf.Clamp(desired.y, minBounds.y + halfHeight, maxBounds.y - halfHeight);
            }

            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime);
        }
    }
}
