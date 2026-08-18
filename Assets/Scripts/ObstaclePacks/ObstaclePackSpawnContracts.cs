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
            float motionStartDelay)
        {
            PackName = packName;
            PackCategory = packCategory;
            RowIndex = rowIndex;
            StartLane = startLane;
            OccupiedLaneCount = occupiedLaneCount;
            MotionStartDelay = motionStartDelay;
        }

        public string PackName { get; }

        public ObstaclePackCategory PackCategory { get; }

        public int RowIndex { get; }

        public int StartLane { get; }

        public int OccupiedLaneCount { get; }

        public float MotionStartDelay { get; }
    }

    /// <summary>
    /// 需要读取包内生成参数的组件实现此接口。
    /// </summary>
    public interface IObstaclePackSpawnReceiver
    {
        void InitializeFromPack(ObstaclePackSpawnContext context);
    }
}
