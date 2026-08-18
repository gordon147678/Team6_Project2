using UnityEngine;

namespace Team6.Project2.ObstaclePacks
{
    [DisallowMultipleComponent]
    public sealed class ObstaclePackVerticalOscillator :
        MonoBehaviour,
        IObstaclePackSpawnReceiver
    {
        [SerializeField] private Transform movingPart;
        [SerializeField] private Vector3 localAxis = Vector3.up;
        [SerializeField] private float travelDistance = 2f;
        [Min(0.01f)]
        [Tooltip("Total up-and-down movement time, excluding the top hold.")]
        [SerializeField] private float cycleDuration = 3f;
        [Min(0f)]
        [Tooltip("Base time at the highest point; pack entries can override it.")]
        [SerializeField] private float topHoldDuration = 1f;

        private Vector3 baseLocalPosition;
        private float activeTopHoldDuration;
        private float elapsed;

        private void Awake()
        {
            if (movingPart == null)
            {
                movingPart = transform;
            }

            baseLocalPosition = movingPart.localPosition;
            activeTopHoldDuration = Mathf.Max(0f, topHoldDuration);
        }

        private void OnEnable()
        {
            ResetMotion();
        }

        public void InitializeFromPack(ObstaclePackSpawnContext context)
        {
            activeTopHoldDuration = context.TopHoldDurationOverride >= 0f
                ? context.TopHoldDurationOverride
                : Mathf.Max(0f, topHoldDuration);

            ResetMotion();
        }

        private void ResetMotion()
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

            float movementDuration = Mathf.Max(0.01f, cycleDuration);
            float halfMovementDuration = movementDuration * 0.5f;
            float holdDuration = Mathf.Max(0f, activeTopHoldDuration);
            float cycleTime = Mathf.Repeat(
                elapsed,
                movementDuration + holdDuration);
            float offset01;

            if (cycleTime < halfMovementDuration)
            {
                float up01 = cycleTime / halfMovementDuration;
                offset01 = 0.5f - 0.5f * Mathf.Cos(up01 * Mathf.PI);
            }
            else if (cycleTime < halfMovementDuration + holdDuration)
            {
                offset01 = 1f;
            }
            else
            {
                float down01 =
                    (cycleTime - halfMovementDuration - holdDuration) /
                    halfMovementDuration;
                offset01 = 0.5f + 0.5f * Mathf.Cos(down01 * Mathf.PI);
            }
            Vector3 axis = localAxis.sqrMagnitude > 0f
                ? localAxis.normalized
                : Vector3.up;

            movingPart.localPosition =
                baseLocalPosition + axis * (offset01 * travelDistance);
        }
    }
}
