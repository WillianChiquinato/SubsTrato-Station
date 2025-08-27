using UnityEngine;

public class BulletDamage : MonoBehaviour
{
    void Start()
    {
        this.gameObject.GetComponent<Collider>().enabled = false;
        Destroy(this.gameObject, 4f);
    }
}
