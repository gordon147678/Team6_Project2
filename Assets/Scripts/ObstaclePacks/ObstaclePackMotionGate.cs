using System.Collections;
using UnityEngine;

namespace Team6.Project2.ObstaclePacks
{
    /// <summary>
    /// 放在叉子、刀或披萨刀预制体上，并把真正控制规律运动的组件拖入列表。
    /// 生成器会按包内编排的延迟时间启用这些组件。
    /// </summary>
    public sealed class ObstaclePackMotionGate :
        MonoBehaviour,
        IObstaclePackSpawnReceiver
    {
        [SerializeField] private Behaviour[] controlledBehaviours;

        private Coroutine enableRoutine;

        private void Awake()
        {
            SetControlledBehavioursEnabled(false);
        }

        public void InitializeFromPack(ObstaclePackSpawnContext context)
        {
            if (enableRoutine != null)
            {
                StopCoroutine(enableRoutine);
                enableRoutine = null;
            }

            SetControlledBehavioursEnabled(false);

            if (context.MotionStartDelay <= 0f)
            {
                SetControlledBehavioursEnabled(true);
                return;
            }

            enableRoutine = StartCoroutine(
                EnableAfterDelay(context.MotionStartDelay));
        }

        private IEnumerator EnableAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            SetControlledBehavioursEnabled(true);
            enableRoutine = null;
        }

        private void SetControlledBehavioursEnabled(bool value)
        {
            if (controlledBehaviours == null)
                return;

            foreach (Behaviour controlledBehaviour in controlledBehaviours)
            {
                if (controlledBehaviour != null && controlledBehaviour != this)
                {
                    controlledBehaviour.enabled = value;
                }
            }
        }
    }
}
