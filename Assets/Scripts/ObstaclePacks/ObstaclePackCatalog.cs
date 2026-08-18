using System;

namespace Team6.Project2.ObstaclePacks
{
    public enum PackObstacleType
    {
        BigPastry,
        SmallPastry,
        Fork,
        Knife,
        PizzaCutter,
        Cheese
    }

    public enum ObstaclePackCategory
    {
        Opening,
        Random
    }

    public sealed class ObstaclePackItem
    {
        private ObstaclePackItem(
            bool isEmpty,
            int emptyLaneCount,
            PackObstacleType obstacleType,
            float motionStartDelay)
        {
            IsEmpty = isEmpty;
            EmptyLaneCount = emptyLaneCount;
            ObstacleType = obstacleType;
            MotionStartDelay = motionStartDelay;
        }

        public bool IsEmpty { get; }

        public int EmptyLaneCount { get; }

        public PackObstacleType ObstacleType { get; }

        /// <summary>
        /// 从这一行生成开始，到障碍物自身规律运动开始之间的秒数。
        /// 障碍物随地图向后移动不受这个值影响。
        /// </summary>
        public float MotionStartDelay { get; }

        public int OccupiedLaneCount =>
            IsEmpty
                ? EmptyLaneCount
                : ObstaclePackCatalog.GetOccupiedLaneCount(ObstacleType);

        public static ObstaclePackItem Empty(int laneCount = 1)
        {
            return new ObstaclePackItem(
                true,
                laneCount,
                default(PackObstacleType),
                0f);
        }

        public static ObstaclePackItem Obstacle(
            PackObstacleType obstacleType,
            float motionStartDelay = 0f)
        {
            return new ObstaclePackItem(
                false,
                0,
                obstacleType,
                motionStartDelay);
        }
    }

    public sealed class ObstaclePackRow
    {
        public ObstaclePackRow(params ObstaclePackItem[] items)
        {
            Items = items ?? Array.Empty<ObstaclePackItem>();
        }

        public ObstaclePackItem[] Items { get; }
    }

    public sealed class ObstaclePackDefinition
    {
        public ObstaclePackDefinition(
            string name,
            ObstaclePackCategory category,
            params ObstaclePackRow[] rows)
        {
            Name = name;
            Category = category;
            Rows = rows ?? Array.Empty<ObstaclePackRow>();
        }

        public string Name { get; }

        public ObstaclePackCategory Category { get; }

        public ObstaclePackRow[] Rows { get; }
    }

    /// <summary>
    /// 所有障碍物包都集中在这个文件中编排。
    /// 每一行所有条目的 OccupiedLaneCount 之和必须等于 LaneCount。
    /// </summary>
    public static class ObstaclePackCatalog
    {
        public const int LaneCount = 5;
        public const int RowsPerPack = 8;

        private static ObstaclePackItem E(int laneCount = 1) =>
            ObstaclePackItem.Empty(laneCount);

        private static ObstaclePackItem BigPastry() =>
            ObstaclePackItem.Obstacle(PackObstacleType.BigPastry);

        private static ObstaclePackItem SmallPastry() =>
            ObstaclePackItem.Obstacle(PackObstacleType.SmallPastry);

        private static ObstaclePackItem Fork(float startsAfter = 0f) =>
            ObstaclePackItem.Obstacle(PackObstacleType.Fork, startsAfter);

        private static ObstaclePackItem Knife(float startsAfter = 0f) =>
            ObstaclePackItem.Obstacle(PackObstacleType.Knife, startsAfter);

        private static ObstaclePackItem PizzaCutter(float startsAfter = 0f) =>
            ObstaclePackItem.Obstacle(PackObstacleType.PizzaCutter, startsAfter);

        private static ObstaclePackItem Cheese() =>
            ObstaclePackItem.Obstacle(PackObstacleType.Cheese);

        private static ObstaclePackRow Row(params ObstaclePackItem[] items) =>
            new ObstaclePackRow(items);

        public static readonly ObstaclePackDefinition OpeningPack =
            new ObstaclePackDefinition(
                "Opening",
                ObstaclePackCategory.Opening,
                Row(E(5)),
                Row(E(), E(), Cheese(), E(), E()),
                Row(E(), E(), BigPastry(), E()),
                Row(SmallPastry(), E(), E(), E(), E()),
                Row(E(), Fork(0.5f), E(), E(), E()),
                Row(E(), E(), Knife(0.75f), E()),
                Row(E(), E(), E(), PizzaCutter(0.4f), E()),
                Row(E(5)));

        public static readonly ObstaclePackDefinition[] RandomPacks =
        {
            // 用户给出的两行示例位于这个包的开头。
            new ObstaclePackDefinition(
                "PastryAndFork",
                ObstaclePackCategory.Random,
                Row(E(), E(), BigPastry(), E()),
                Row(SmallPastry(), Fork(0.75f), E(), E(), E()),
                Row(E(), E(), E(), E(), Cheese()),
                Row(E(5)),
                Row(E(), SmallPastry(), E(), SmallPastry(), E()),
                Row(Fork(0.25f), E(), E(), E(), Fork(1f)),
                Row(E(), E(), BigPastry(), E()),
                Row(E(5))),

            new ObstaclePackDefinition(
                "KnifeAndPizzaCutter",
                ObstaclePackCategory.Random,
                Row(Knife(0.5f), E(), E(), E()),
                Row(E(5)),
                Row(E(), PizzaCutter(0.25f), E(), E(), E()),
                Row(E(), E(), E(), Knife(1f)),
                Row(Cheese(), E(), E(), E(), E()),
                Row(E(), E(), PizzaCutter(0.75f), E(), E()),
                Row(E(), Knife(0f), E(), E()),
                Row(E(5))),

            new ObstaclePackDefinition(
                "Mixed",
                ObstaclePackCategory.Random,
                Row(E(), Cheese(), E(), Cheese(), E()),
                Row(BigPastry(), E(), SmallPastry(), E()),
                Row(E(5)),
                Row(E(), Fork(1f), E(), PizzaCutter(0.5f), E()),
                Row(E(), E(), Knife(0.25f), E()),
                Row(SmallPastry(), E(), E(), E(), Cheese()),
                Row(E(), E(), BigPastry(), E()),
                Row(E(5)))
        };

        public static int GetOccupiedLaneCount(PackObstacleType obstacleType)
        {
            switch (obstacleType)
            {
                case PackObstacleType.BigPastry:
                case PackObstacleType.Knife:
                    return 2;

                case PackObstacleType.SmallPastry:
                case PackObstacleType.Fork:
                case PackObstacleType.PizzaCutter:
                case PackObstacleType.Cheese:
                    return 1;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(obstacleType),
                        obstacleType,
                        null);
            }
        }

        public static bool HasTimedMotion(PackObstacleType obstacleType)
        {
            return obstacleType == PackObstacleType.Fork ||
                   obstacleType == PackObstacleType.Knife ||
                   obstacleType == PackObstacleType.PizzaCutter;
        }
    }
}
