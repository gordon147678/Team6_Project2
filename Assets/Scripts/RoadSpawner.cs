using UnityEngine;
using System.Collections.Generic;

public class RoadSpawner : MonoBehaviour
{
    public GameObject roadPrefab;

    // 一块道路真正的Z长度
    public float segmentLength = 20f;

    // 同时存在几块
    public int segmentCount = 6;

    // 道路移动到这里之后循环到最前面
    public float recycleZ = -10f;


    private List<GameObject> roads =
        new List<GameObject>();



    void Start()
    {
        // 一开始连续生成
        for (int i = 0; i < segmentCount; i++)
        {
            GameObject road = Instantiate(
                roadPrefab,
                new Vector3(
                    0,
                    0,
                    i * segmentLength
                ),
                Quaternion.identity
            );

            roads.Add(road);
        }
    }



    void Update()
    {
        GameObject firstRoad = roads[0];


        // 第一块已经跑到玩家后面
        if (firstRoad.transform.position.z < recycleZ)
        {
            // 找到最后一块道路
            GameObject lastRoad =
                roads[roads.Count - 1];


            // 把第一块直接移动到最后一块后面
            firstRoad.transform.position =
                new Vector3(
                    0,
                    firstRoad.transform.position.y,
                    lastRoad.transform.position.z
                    + segmentLength
                );


            // 更新列表顺序
            roads.RemoveAt(0);

            roads.Add(firstRoad);
        }
    }
}