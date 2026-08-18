using UnityEngine;

public class PlayerCollision : MonoBehaviour
{

    private bool isDead = false;



    private void OnTriggerEnter(Collider other)
    {

        if (isDead)
            return;


        if (other.CompareTag("Obstacle"))
        {

            isDead = true;

            Debug.Log("Game Over");

            Time.timeScale = 0;

        }

    }

}