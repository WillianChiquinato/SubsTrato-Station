using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("Sounds")]
    public AudioSource EnemyDeathSound;
    public AudioSource EnemyIdleSound;
    private bool hasPlayedDeathSound = false;


    [Header("Knockback")]
    public float knockbackDuration = 0.3f;
    private float knockbackTimer;
    private Vector3 knockbackVelocity;


    [Header("States")]
    public bool IsMoving;
    public bool IsFear;
    public bool IsEscape;


    [Header("Settings")]
    public float escapeTrueDistance = 10f;
    public float escapeDistance = 10f;


    [Header("Target Settings")]
    [SerializeField] private float distanceToPlayer;
    public bool isFleeing;
    public NavMeshAgent Agent;
    public Animator animator;
    public Health health;
    public PlayerMoviment playerDetect;
    public ItemDrop itemDrop;
    public GameObject itemDropActive;

    public bool canMove
    {
        get
        {
            return animator.GetBool("canMove");
        }
        set
        {
            animator.SetBool("canMove", value);
        }
    }

    void Start()
    {
        health = GetComponent<Health>();
        Agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        itemDrop = GetComponent<ItemDrop>();
    }

    void Update()
    {
        animator.SetBool("IsMoving", !Agent.isStopped);
        animator.SetBool("IsAlive", health.isAlive);
        animator.SetBool("IsFear", IsFear);

        if (!health.isAlive)
        {
            if (EnemyDeathSound != null && !hasPlayedDeathSound)
            {
                AudioSource.PlayClipAtPoint(EnemyDeathSound.clip, transform.position);
            }
            hasPlayedDeathSound = true;
            Agent.isStopped = true;
            canMove = false;

            if (!itemDropActive)
            {
                itemDrop.DropItem();
                itemDropActive = itemDrop.gameObject;
            }

            GetComponent<CharacterController>().enabled = false;
            GetComponent<CapsuleCollider>().enabled = false;

            if (EnemyIdleSound != null && EnemyIdleSound.isPlaying)
            {
                EnemyIdleSound.Stop();
            }
            return;
        }

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("HitBlock"))
        {
            Vector3 hitDirection = (playerDetect.transform.position - transform.position).normalized;
            hitDirection.y = 0;
            if (hitDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(hitDirection);
                transform.rotation = targetRotation;
            }
            
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
            {
                Agent.isStopped = false;
                canMove = true;
            }
            else
            {
                Agent.isStopped = true;
                canMove = false;
            }
            return;
        }

        if (playerDetect == null) return;

        distanceToPlayer = Vector3.Distance(transform.position, playerDetect.transform.position);

        if (distanceToPlayer <= escapeTrueDistance)
        {
            if (!isFleeing)
            {
                isFleeing = true;
                FleeFromPlayer();
            }

            if (isFleeing && !Agent.pathPending && Agent.remainingDistance <= Agent.stoppingDistance)
            {
                FleeFromPlayer();
            }
        }
        else
        {
            if (isFleeing)
            {
                Agent.isStopped = true;
                Agent.ResetPath();
                IsEscape = false;
                IsFear = true;

                escapeTrueDistance = 30f;
                isFleeing = false;
            }
        }
    }


    void FleeFromPlayer()
    {
        escapeTrueDistance += 20f;
        Vector3 fleeDirection = (transform.position - playerDetect.transform.position).normalized;

        // Desvio para evitar travar sempre em linha reta.
        fleeDirection += new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
        fleeDirection.Normalize();

        Vector3 fleeTarget = transform.position + fleeDirection * escapeDistance;

        if (NavMesh.SamplePosition(fleeTarget, out NavMeshHit hit, 20f, NavMesh.AllAreas))
        {
            Agent.isStopped = false;
            Agent.SetDestination(hit.position);
            canMove = true;
            IsEscape = true;
            IsFear = false;
        }
        else
        {
            Agent.isStopped = true;
            Agent.ResetPath();
            canMove = false;
            IsEscape = false;
            IsFear = true;
        }
    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        direction.y = 0.2f;
        knockbackVelocity = direction.normalized * force;
        knockbackTimer = knockbackDuration;
        Agent.isStopped = true;
        Agent.ResetPath();
        canMove = false;
    }

    public bool IsBeingKnockedBack()
    {
        return knockbackTimer > 0;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            animator.SetTrigger("BulletBlock");
        }
    }
}
