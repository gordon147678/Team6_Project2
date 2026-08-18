using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Team6.Project2.ObstaclePacks
{
    public sealed class ObstaclePackSpawner : MonoBehaviour
    {
        public enum ForwardMovementMode
        {
            AddPackMover,
            PrefabHandlesMovement
        }

        [Serializable]
        public sealed class ObstaclePrefabBinding
        {
            public PackObstacleType obstacleType;
            public GameObject prefab;
            public ForwardMovementMode forwardMovementMode =
                ForwardMovementMode.AddPackMover;
        }

        [Header("障碍物预制体")]
        [SerializeField] private ObstaclePrefabBinding[] prefabBindings;

        [Header("道路")]
        [SerializeField] private int laneCount = ObstaclePackCatalog.LaneCount;
        [SerializeField] private float laneWidth = 2f;

        [Header("逐行生成")]
        [SerializeField] private float spawnY;
        [SerializeField] private float spawnZ = 50f;
        [SerializeField] private float rowInterval = 1f;
        [SerializeField] private Transform spawnedObjectParent;

        [Header("前进模拟")]
        [SerializeField] private float obstacleSpeed = 10f;
        [SerializeField] private float destroyZ = -10f;

        [Header("随机规则")]
        [Range(0f, 1f)]
        [SerializeField] private float randomPackCheeseProbability = 0.5f;
        [SerializeField] private bool preventImmediatePackRepeat = true;
        [SerializeField] private bool useFixedRandomSeed;
        [SerializeField] private int randomSeed = 12345;

        [Header("启动")]
        [SerializeField] private bool playOnStart = true;

        private readonly Dictionary<PackObstacleType, ObstaclePrefabBinding>
            bindingsByType =
                new Dictionary<PackObstacleType, ObstaclePrefabBinding>();

        private System.Random random;
        private Coroutine spawnRoutine;
        private int lastRandomPackIndex = -1;

        private void Start()
        {
            if (playOnStart)
            {
                BeginSpawning();
            }
        }

        private void OnDisable()
        {
            StopSpawning();
        }

        public void BeginSpawning()
        {
            if (spawnRoutine != null)
                return;

            if (!TryValidateConfiguration(out string error))
            {
                Debug.LogError(error, this);
                return;
            }

            BuildBindingLookup();
            ResetRandom();
            lastRandomPackIndex = -1;
            spawnRoutine = StartCoroutine(SpawnSequence());
        }

        public void StopSpawning()
        {
            if (spawnRoutine == null)
                return;

            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        public bool TryValidateConfiguration(out string error)
        {
            if (laneCount != ObstaclePackCatalog.LaneCount)
            {
                error =
                    $"包目录按 {ObstaclePackCatalog.LaneCount} 条道路编排，" +
                    $"但生成器当前配置为 {laneCount} 条。";
                return false;
            }

            if (laneWidth <= 0f)
            {
                error = "Lane Width 必须大于 0。";
                return false;
            }

            if (rowInterval <= 0f)
            {
                error = "Row Interval 必须大于 0。";
                return false;
            }

            if (!TryValidateBindings(out error))
                return false;

            if (!TryValidatePack(ObstaclePackCatalog.OpeningPack, out error))
                return false;

            ObstaclePackDefinition[] randomPacks =
                ObstaclePackCatalog.RandomPacks;

            if (randomPacks == null || randomPacks.Length == 0)
            {
                error = "至少需要一个随机包。";
                return false;
            }

            foreach (ObstaclePackDefinition pack in randomPacks)
            {
                if (!TryValidatePack(pack, out error))
                    return false;
            }

            error = null;
            return true;
        }

        private IEnumerator SpawnSequence()
        {
            yield return SpawnPack(ObstaclePackCatalog.OpeningPack);

            while (true)
            {
                ObstaclePackDefinition nextPack = PickRandomPack();
                yield return SpawnPack(nextPack);
            }
        }

        private IEnumerator SpawnPack(ObstaclePackDefinition pack)
        {
            for (int rowIndex = 0; rowIndex < pack.Rows.Length; rowIndex++)
            {
                SpawnRow(pack, rowIndex, pack.Rows[rowIndex]);
                yield return new WaitForSeconds(rowInterval);
            }
        }

        private void SpawnRow(
            ObstaclePackDefinition pack,
            int rowIndex,
            ObstaclePackRow row)
        {
            int laneCursor = 0;

            foreach (ObstaclePackItem item in row.Items)
            {
                int occupiedLanes = item.OccupiedLaneCount;

                if (item.IsEmpty)
                {
                    laneCursor += occupiedLanes;
                    continue;
                }

                if (ShouldSpawn(pack.Category, item.ObstacleType))
                {
                    SpawnObstacle(
                        pack,
                        rowIndex,
                        laneCursor,
                        occupiedLanes,
                        item);
                }

                // 奶酪没有通过概率判定时，格子仍然属于本行编排的一部分。
                laneCursor += occupiedLanes;
            }
        }

        private void SpawnObstacle(
            ObstaclePackDefinition pack,
            int rowIndex,
            int startLane,
            int occupiedLanes,
            ObstaclePackItem item)
        {
            ObstaclePrefabBinding binding = bindingsByType[item.ObstacleType];

            float centerLane =
                startLane + (occupiedLanes - 1) * 0.5f;

            float x =
                (centerLane - (laneCount - 1) * 0.5f) * laneWidth;

            GameObject instance = Instantiate(
                binding.prefab,
                new Vector3(x, spawnY, spawnZ),
                binding.prefab.transform.rotation,
                spawnedObjectParent);

            if (binding.forwardMovementMode == ForwardMovementMode.AddPackMover)
            {
                ObstaclePackMover mover =
                    instance.GetComponent<ObstaclePackMover>();

                if (mover == null)
                {
                    mover = instance.AddComponent<ObstaclePackMover>();
                }

                mover.Configure(obstacleSpeed, destroyZ);
            }

            ObstaclePackSpawnContext context =
                new ObstaclePackSpawnContext(
                    pack.Name,
                    pack.Category,
                    rowIndex,
                    startLane,
                    occupiedLanes,
                    laneCount,
                    laneWidth,
                    item.MotionStartDelay,
                    item.TopHoldDurationOverride);

            NotifySpawnReceivers(instance, context);
        }

        private static void NotifySpawnReceivers(
            GameObject instance,
            ObstaclePackSpawnContext context)
        {
            MonoBehaviour[] behaviours =
                instance.GetComponentsInChildren<MonoBehaviour>(true);

            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IObstaclePackSpawnReceiver receiver)
                {
                    receiver.InitializeFromPack(context);
                }
            }
        }

        private bool ShouldSpawn(
            ObstaclePackCategory category,
            PackObstacleType obstacleType)
        {
            if (obstacleType != PackObstacleType.Cheese)
                return true;

            if (category == ObstaclePackCategory.Opening)
                return true;

            return random.NextDouble() < randomPackCheeseProbability;
        }

        private ObstaclePackDefinition PickRandomPack()
        {
            ObstaclePackDefinition[] packs =
                ObstaclePackCatalog.RandomPacks;

            int nextIndex;

            if (!preventImmediatePackRepeat ||
                packs.Length == 1 ||
                lastRandomPackIndex < 0)
            {
                nextIndex = random.Next(0, packs.Length);
            }
            else
            {
                nextIndex = random.Next(0, packs.Length - 1);

                if (nextIndex >= lastRandomPackIndex)
                {
                    nextIndex++;
                }
            }

            lastRandomPackIndex = nextIndex;
            return packs[nextIndex];
        }

        private bool TryValidateBindings(out string error)
        {
            if (prefabBindings == null)
            {
                error = "Prefab Bindings 尚未配置。";
                return false;
            }

            HashSet<PackObstacleType> foundTypes =
                new HashSet<PackObstacleType>();

            foreach (ObstaclePrefabBinding binding in prefabBindings)
            {
                if (binding == null || binding.prefab == null)
                {
                    error = "Prefab Bindings 中存在空项。";
                    return false;
                }

                if (!foundTypes.Add(binding.obstacleType))
                {
                    error = $"障碍物类型 {binding.obstacleType} 被重复绑定。";
                    return false;
                }
            }

            foreach (PackObstacleType obstacleType in
                     Enum.GetValues(typeof(PackObstacleType)))
            {
                if (!foundTypes.Contains(obstacleType))
                {
                    error = $"缺少障碍物类型 {obstacleType} 的预制体。";
                    return false;
                }
            }

            error = null;
            return true;
        }

        private static bool TryValidatePack(
            ObstaclePackDefinition pack,
            out string error)
        {
            if (pack == null)
            {
                error = "包定义不能为空。";
                return false;
            }

            if (pack.Rows.Length != ObstaclePackCatalog.RowsPerPack)
            {
                error =
                    $"包 {pack.Name} 必须有 " +
                    $"{ObstaclePackCatalog.RowsPerPack} 行，" +
                    $"当前有 {pack.Rows.Length} 行。";
                return false;
            }

            for (int rowIndex = 0; rowIndex < pack.Rows.Length; rowIndex++)
            {
                ObstaclePackRow row = pack.Rows[rowIndex];

                if (row == null || row.Items == null)
                {
                    error = $"包 {pack.Name} 的第 {rowIndex + 1} 行为空定义。";
                    return false;
                }

                int laneTotal = 0;

                foreach (ObstaclePackItem item in row.Items)
                {
                    if (item == null)
                    {
                        error =
                            $"包 {pack.Name} 的第 {rowIndex + 1} 行包含空条目。";
                        return false;
                    }

                    if (item.OccupiedLaneCount <= 0)
                    {
                        error =
                            $"包 {pack.Name} 的第 {rowIndex + 1} 行" +
                            "包含占用格数小于 1 的条目。";
                        return false;
                    }

                    if (!item.IsEmpty &&
                        item.MotionStartDelay < 0f)
                    {
                        error =
                            $"包 {pack.Name} 的第 {rowIndex + 1} 行" +
                            "包含负数启动延迟。";
                        return false;
                    }

                    if (!item.IsEmpty &&
                        !ObstaclePackCatalog.HasTimedMotion(item.ObstacleType) &&
                        item.MotionStartDelay > 0f)
                    {
                        error =
                            $"包 {pack.Name} 的第 {rowIndex + 1} 行中，" +
                            $"{item.ObstacleType} 不是可延迟运动的障碍物。";
                        return false;
                    }

                    laneTotal += item.OccupiedLaneCount;
                }

                if (laneTotal != ObstaclePackCatalog.LaneCount)
                {
                    error =
                        $"包 {pack.Name} 的第 {rowIndex + 1} 行共占 " +
                        $"{laneTotal} 格，应为 {ObstaclePackCatalog.LaneCount} 格。";
                    return false;
                }
            }

            error = null;
            return true;
        }

        private void BuildBindingLookup()
        {
            bindingsByType.Clear();

            foreach (ObstaclePrefabBinding binding in prefabBindings)
            {
                bindingsByType.Add(binding.obstacleType, binding);
            }
        }

        private void ResetRandom()
        {
            random = useFixedRandomSeed
                ? new System.Random(randomSeed)
                : new System.Random(
                    unchecked(Environment.TickCount ^ GetInstanceID()));
        }
    }
}
