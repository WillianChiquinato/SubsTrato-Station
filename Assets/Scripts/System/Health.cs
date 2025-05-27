using UnityEngine;

public class Health : MonoBehaviour
{
    public bool canReceiveKnockback = false;

    public float health;
    public bool isDead = false;
    public bool isAlive = true;

    [SerializeField]
    public bool isInvicible = false;
    private float timeSincehit;
    [SerializeField]
    private float invicibilityTimer = 0.5f;

    void Update()
    {
        if (isInvicible)
        {
            if (timeSincehit > invicibilityTimer)
            {
                // Remove a invencibilidade
                isInvicible = false;
                timeSincehit = 0;
            }

            timeSincehit += Time.deltaTime;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isAlive && !isInvicible)
        {
            health -= damage;
            isInvicible = true;

            if (health <= 0)
            {
                isAlive = false;
            }
        }
    }
}
