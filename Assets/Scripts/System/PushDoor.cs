using UnityEngine;

public class PushDoor : MonoBehaviour
{
    public GameObject[] door;

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Target"))
        {
            if (door == null) return;

            foreach (GameObject portas in door)
            {
                Animator anim = portas.GetComponent<Animator>();
                if (anim != null)
                {
                    anim.SetTrigger("Open");
                }
                
                BoxCollider bc = portas.GetComponent<BoxCollider>();
                if (bc != null)
                {
                    bc.enabled = false;
                }
            }
            Debug.Log("TRIGGERR");
        }
    }
}
