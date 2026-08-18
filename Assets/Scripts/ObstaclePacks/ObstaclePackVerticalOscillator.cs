using UnityEngine;

namespace Team6.Project2.ObstaclePacks
{
    [DisallowMultipleComponent]
    public sealed class ObstaclePackVerticalOscillator : MonoBehaviour
    {
        [SerializeField] private Transform movingPart;
        [SerializeField] private Vector3 localAxis = Vector3.up;
        [SerializeField] private float travelDistance = 2f;
        [SerializeField] private float cycleDuration = 1.5f;

        private Vector3 baseLocalPosition;
        private float elapsed;

        private void Awake()
        {
            if (movingPart == null)
            {
                movingPart = transform;
            }

            baseLocalPosition = movingPart.localPosition;
        }

        private void OnEnable()
        {
            elapsed = 0f;

            if (movingPart != null)
            {
                movingPart.localPosition = baseLocalPosition;
            }
        }

        private void Update()
        {
            if (movingPart == null)
                return;

            elapsed += Time.deltaTime;

            float safeDuration = Mathf.Max(0.01f, cycleDuration);
            float phase = Mathf.Repeat(elapsed / safeDuration, 1f);
            float offset01 = 0.5f - 0.5f * Mathf.Cos(phase * Mathf.PI * 2f);
            Vector3 axis = localAxis.sqrMagnitude > 0f
                ? localAxis.normalized
                : Vector3.up;

            movingPart.localPosition =
                baseLocalPosition + axis * (offset01 * travelDistance);
        }
    }
}
