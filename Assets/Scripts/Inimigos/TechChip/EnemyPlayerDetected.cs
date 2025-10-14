using UnityEngine;

public class EnemyPlayerDetected : MonoBehaviour
{
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GetComponentInParent<Enemy>().playerDetect = other.GetComponent<PlayerMoviment>();
        }
    }
}
