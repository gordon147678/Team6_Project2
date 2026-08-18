using UnityEngine;

public class RoadMovement : MonoBehaviour
{

    public float speed = 10f;


    void Update()
    {

        transform.position +=
            Vector3.back * speed * Time.deltaTime;

    }

}