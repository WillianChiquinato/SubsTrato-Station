using UnityEngine;

public class PushDoor : MonoBehaviour
{
    public GameObject door;

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Target"))
        {
            door.GetComponent<Animator>().SetTrigger("Open");
            door.GetComponent<BoxCollider>().enabled = false;
            Debug.Log("TRIGGERR");
        }
    }
}
