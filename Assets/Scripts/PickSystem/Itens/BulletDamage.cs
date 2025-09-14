using UnityEngine;

public class BulletDamage : MonoBehaviour
{
    void Start()
    {
        Destroy(this.gameObject, 4f);
    }
}
