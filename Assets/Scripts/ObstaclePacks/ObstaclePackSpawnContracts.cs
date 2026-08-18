namespace Team6.Project2.ObstaclePacks
{
    public readonly struct ObstaclePackSpawnContext
    {
        public ObstaclePackSpawnContext(
            string packName,
            ObstaclePackCategory packCategory,
            int rowIndex,
            int startLane,
            int occupiedLaneCount,
            int laneCount,
            float laneWidth,
            float motionStartDelay,
            float topHoldDurationOverride)
        {
            PackName = packName;
            PackCategory = packCategory;
            RowIndex = rowIndex;
            StartLane = startLane;
            OccupiedLaneCount = occupiedLaneCount;
            LaneCount = laneCount;
            LaneWidth = laneWidth;
            MotionStartDelay = motionStartDelay;
            TopHoldDurationOverride = topHoldDurationOverride;
        }

        public string PackName { get; }

        public ObstaclePackCategory PackCategory { get; }

        public int RowIndex { get; }

        public int StartLane { get; }

        public int OccupiedLaneCount { get; }

        public int LaneCount { get; }

        public float LaneWidth { get; }

        public float MotionStartDelay { get; }

        public float TopHoldDurationOverride { get; }
    }

    /// <summary>
    /// 需要读取包内生成参数的组件实现此接口。
    /// </summary>
    public interface IObstaclePackSpawnReceiver
    {
        void InitializeFromPack(ObstaclePackSpawnContext context);
    }
}
