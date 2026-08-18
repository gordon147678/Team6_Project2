using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public TMP_Text scoreText;

    // 拖一个正在移动的 RoadSegment 进来
    public RoadMovement roadMovement;

    // 每移动多少距离加1分
    public float distancePerScore = 5f;


    private float distance = 0f;

    private int score = 0;



    void Start()
    {
        UpdateUI();
    }



    void Update()
    {
        if (roadMovement == null)
            return;


        // 累计道路移动距离
        distance +=
            roadMovement.speed *
            Time.deltaTime;


        // 每移动5单位 +1分
        while (distance >= distancePerScore)
        {
            distance -= distancePerScore;

            score++;

            UpdateUI();
        }
    }



    void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text =
                "Score: " + score;
        }
    }



    // 获取当前分数
    public int GetScore()
    {
        return score;
    }
}