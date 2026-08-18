using UnityEngine;

namespace Team6.Project2.ObstaclePacks
{
    /// <summary>
    /// 新包系统自带的前进模拟组件，不依赖现有 ObstacleMover。
    /// </summary>
    public sealed class ObstaclePackMover : MonoBehaviour
    {
        [SerializeField] private float speed = 10f;
        [SerializeField] private float destroyZ = -10f;

        public void Configure(float newSpeed, float newDestroyZ)
        {
            speed = Mathf.Max(0f, newSpeed);
            destroyZ = newDestroyZ;
        }

        private void Update()
        {
            transform.position += Vector3.back * speed * Time.deltaTime;

            if (transform.position.z < destroyZ)
            {
                Destroy(gameObject);
            }
        }
    }
}
