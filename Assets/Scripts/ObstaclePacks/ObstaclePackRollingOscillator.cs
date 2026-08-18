using UnityEngine;

namespace Team6.Project2.ObstaclePacks
{
    [DisallowMultipleComponent]
    public sealed class ObstaclePackRollingOscillator :
        MonoBehaviour,
        IObstaclePackSpawnReceiver
    {
        [SerializeField] private Transform movingPart;
        [SerializeField] private Vector3 localTravelAxis = Vector3.right;
        [SerializeField] private float travelDistance = 8f;
        [SerializeField] private float cycleDuration = 2f;

        private Vector3 baseLocalPosition;
        private Vector3 authoredLocalPosition;
        private Quaternion baseLocalRotation;
        private float elapsed;

        private void Awake()
        {
            if (movingPart == null)
            {
                movingPart = transform;
            }

            baseLocalPosition = movingPart.localPosition;
            authoredLocalPosition = baseLocalPosition;
            baseLocalRotation = movingPart.localRotation;
        }

        private void OnEnable()
        {
            ResetMotion();
        }

        public void InitializeFromPack(ObstaclePackSpawnContext context)
        {
            float centerLane =
                context.StartLane + (context.OccupiedLaneCount - 1) * 0.5f;
            float spawnOffsetFromRoadCenter =
                (centerLane - (context.LaneCount - 1) * 0.5f) *
                context.LaneWidth;

            baseLocalPosition =
                authoredLocalPosition -
                GetTravelAxis() * spawnOffsetFromRoadCenter;
            travelDistance =
                Mathf.Max(0f, (context.LaneCount - 1) * context.LaneWidth);

            ResetMotion();
        }

        private void ResetMotion()
        {
            elapsed = 0f;

            if (movingPart != null)
            {
                movingPart.localPosition =
                    baseLocalPosition -
                    GetTravelAxis() * (travelDistance * 0.5f);
                movingPart.localRotation = baseLocalRotation;
            }
        }

        private void Update()
        {
            if (movingPart == null)
                return;

            elapsed += Time.deltaTime;

            float safeDuration = Mathf.Max(0.01f, cycleDuration);
            float phase = Mathf.Repeat(elapsed / safeDuration, 1f);
            float position01 = phase < 0.5f
                ? phase * 2f
                : (1f - phase) * 2f;
            float offset = (position01 - 0.5f) * travelDistance;

            movingPart.localPosition =
                baseLocalPosition + GetTravelAxis() * offset;
            movingPart.localRotation = baseLocalRotation;
        }

        private Vector3 GetTravelAxis()
        {
            return localTravelAxis.sqrMagnitude > 0f
                ? localTravelAxis.normalized
                : Vector3.right;
        }
    }
}
