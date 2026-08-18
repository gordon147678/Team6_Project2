using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    public float speed = 10f;

    public float destroyZ = -60f;


    void Update()
    {
        transform.position +=
            Vector3.back *
            speed *
            Time.deltaTime;


        if (transform.position.z < destroyZ)
        {
            Destroy(gameObject);
        }
    }
}