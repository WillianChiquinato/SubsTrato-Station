using UnityEngine;

public class PushDoor : MonoBehaviour
{
    public GameObject door;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Target"))
        {
            door.GetComponent<Animator>().SetTrigger("Open");
        }
    }
}
