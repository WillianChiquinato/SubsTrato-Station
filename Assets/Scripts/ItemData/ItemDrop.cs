using System.Collections.Generic;
using UnityEngine;

public class ItemDrop : MonoBehaviour
{
    [SerializeField] private GameObject[] drops;

    public void DropItem()
    {
        GameObject newDrop = Instantiate(drops[Random.Range(0, drops.Length)], transform.position, Quaternion.identity);

        Vector3 randomDirection = Random.insideUnitSphere;
        randomDirection.y = Mathf.Abs(randomDirection.y) + 1.7f; // Garante que sempre vá um pouco pra cima
        randomDirection.Normalize();

        Vector3 finalVelocity = randomDirection * Random.Range(5f, 7f);
        newDrop.GetComponent<Rigidbody>().linearVelocity = finalVelocity;
    }
}
