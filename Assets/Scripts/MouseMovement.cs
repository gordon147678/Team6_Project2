using UnityEngine;

public class MouseMovement : MonoBehaviour
{
    public float laneWidth = 1f;

    // 换路速度
    public float moveSpeed = 5f;


    // 当前路线
    // -2 -1 0 1 2
    private int currentLane = 0;


    private float targetX;


    // 是否正在换路
    private bool isChangingLane = false;



    void Start()
    {
        targetX = transform.position.x;
    }



    void Update()
    {

        // =========================
        // 只有换路完成才能再次输入
        // =========================

        if (!isChangingLane)
        {

            if (Input.GetKeyDown(KeyCode.A))
            {
                ChangeLane(-1);
            }

            else if (Input.GetKeyDown(KeyCode.D))
            {
                ChangeLane(1);
            }

        }



        // =========================
        // 正在换路
        // =========================

        if (isChangingLane)
        {

            Vector3 pos = transform.position;


            pos.x = Mathf.MoveTowards(
                pos.x,
                targetX,
                moveSpeed * Time.deltaTime
            );


            transform.position = pos;



            // 已经到达目标路线
            if (Mathf.Abs(transform.position.x - targetX) < 0.001f)
            {

                // 强制对齐路线中心
                Vector3 finalPos = transform.position;

                finalPos.x = targetX;

                transform.position = finalPos;


                // 可以再次输入
                isChangingLane = false;

            }

        }

    }



    void ChangeLane(int direction)
    {

        int newLane =
            Mathf.Clamp(
                currentLane + direction,
                -2,
                2
            );


        // 已经在最边缘
        // 不进行移动
        if (newLane == currentLane)
            return;


        currentLane = newLane;


        targetX =
            currentLane * laneWidth;


        // 锁定输入
        isChangingLane = true;

    }

}