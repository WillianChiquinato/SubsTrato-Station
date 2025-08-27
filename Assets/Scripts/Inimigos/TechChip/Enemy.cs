using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Knockback")]
    public float knockbackDuration = 0.3f;
    private float knockbackTimer;
    private Vector3 knockbackVelocity;

    public Health health;

    void Start()
    {
        health = GetComponent<Health>();
    }

    void Update()
    {
        if (!health.isAlive)
        {
            health.isDead = true;
            StartCoroutine(TimingToRevive());
        }
    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        direction.y = 0.2f;
        knockbackVelocity = direction.normalized * force;
        knockbackTimer = knockbackDuration;
    }

    public bool IsBeingKnockedBack()
    {
        return knockbackTimer > 0;
    }

    IEnumerator TimingToRevive()
    {
        yield return new WaitForSeconds(3f);
        health.isAlive = true;
        health.isDead = false;
    }
}
