using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    // 所有障碍Prefab
    public GameObject[] obstaclePrefabs;


    // 5路线
    public int laneCount = 5;

    // 单路宽度
    public float laneWidth = 1f;


    // 障碍出现的位置
    public float spawnZ = 80f;


    // 每组障碍之间的时间
    public float spawnInterval = 1.5f;


    private float timer;



    void Update()
    {
        timer += Time.deltaTime;


        if (timer >= spawnInterval)
        {
            SpawnObstacle();

            timer = 0;
        }
    }



    void SpawnObstacle()
    {
        if (obstaclePrefabs.Length == 0)
            return;


        // =========================
        // 随机选择障碍
        // =========================

        GameObject prefab =
            obstaclePrefabs[
                Random.Range(
                    0,
                    obstaclePrefabs.Length
                )
            ];


        ObstacleData data =
            prefab.GetComponent<ObstacleData>();


        if (data == null)
            return;


        int obstacleLaneCount =
            data.laneCount;



        // =========================
        // 随机选择起始路线
        // =========================

        int startLane =
            Random.Range(
                0,
                laneCount - obstacleLaneCount + 1
            );



        // =========================
        // 算出障碍中心X位置
        // =========================

        float centerLane =
            startLane
            + (obstacleLaneCount - 1) * 0.5f;


        float x =
            (centerLane - 2f)
            * laneWidth;



        // =========================
        // 生成
        // =========================

        Instantiate(
            prefab,
            new Vector3(
                x,
                0,
                spawnZ
            ),
            prefab.transform.rotation
        );
    }
}