using UnityEngine;

public class OrbeObject : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OrbSpawner.Instance.collectedOrbs++;
            Destroy(gameObject);
        }
    }

    void Update()
    {
        float floatAmplitude = 0.5f;
        float floatFrequency = 4f;

        Vector3 newPosition = transform.position;
        newPosition.y += Mathf.Sin(Time.time * floatFrequency) * floatAmplitude * Time.fixedDeltaTime;
        transform.position = newPosition;
    }
}
