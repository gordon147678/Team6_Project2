using UnityEngine;

namespace Team6.Project2.ObstaclePacks
{
    [DisallowMultipleComponent]
    public sealed class ObstaclePackRollingOscillator : MonoBehaviour
    {
        [SerializeField] private Transform movingPart;
        [SerializeField] private Vector3 localTravelAxis = Vector3.forward;
        [SerializeField] private Vector3 localRotationAxis = Vector3.right;
        [SerializeField] private float travelDistance = 2f;
        [SerializeField] private float cycleDuration = 2f;
        [SerializeField] private float wheelRadius = 0.6f;

        private Vector3 baseLocalPosition;
        private Quaternion baseLocalRotation;
        private float previousOffset;
        private float accumulatedAngle;
        private float elapsed;

        private void Awake()
        {
            if (movingPart == null)
            {
                movingPart = transform;
            }

            baseLocalPosition = movingPart.localPosition;
            baseLocalRotation = movingPart.localRotation;
        }

        private void OnEnable()
        {
            elapsed = 0f;
            previousOffset = -travelDistance * 0.5f;
            accumulatedAngle = 0f;

            if (movingPart != null)
            {
                movingPart.localPosition =
                    baseLocalPosition + GetTravelAxis() * previousOffset;
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
            float distanceDelta = offset - previousOffset;
            float safeRadius = Mathf.Max(0.01f, Mathf.Abs(wheelRadius));

            accumulatedAngle += distanceDelta / safeRadius * Mathf.Rad2Deg;

            movingPart.localPosition =
                baseLocalPosition + GetTravelAxis() * offset;
            movingPart.localRotation =
                baseLocalRotation *
                Quaternion.AngleAxis(accumulatedAngle, GetRotationAxis());

            previousOffset = offset;
        }

        private Vector3 GetTravelAxis()
        {
            return localTravelAxis.sqrMagnitude > 0f
                ? localTravelAxis.normalized
                : Vector3.forward;
        }

        private Vector3 GetRotationAxis()
        {
            return localRotationAxis.sqrMagnitude > 0f
                ? localRotationAxis.normalized
                : Vector3.right;
        }
    }
}
