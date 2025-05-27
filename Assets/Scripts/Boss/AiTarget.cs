using UnityEngine;
using UnityEngine.AI;

public class AiTarget : MonoBehaviour
{
    [Header("Target Settings")]
    public SphereCollider damageCollider;
    public Transform target;
    public bool targetBool;
    public float AttackDistance;

    private NavMeshAgent agent;
    private Animator animator;
    [SerializeField] private float distanceToTarget;

    [Header("States")]
    public bool IsMoving;
    public bool isAttacking;
    public bool IsPatrolling;

    [Header("Patrulha Targets")]
    public Transform[] PatrulhaTargets;
    private int currentPatrolIndex = 0;

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
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        damageCollider = GetComponentInChildren<SphereCollider>();
        damageCollider.enabled = false;

        //Debug por enquanto.
        // target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        animator.SetBool("isMoving", IsMoving);
        animator.SetBool("isAttacking", isAttacking);
        animator.SetBool("Patrulha", IsPatrolling);
        animator.SetBool("Target", targetBool);

        if (target != null)
        {
            if (!target.GetComponent<Health>().isAlive)
            {
                target = null;
            }
        }

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.5f && animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.8f)
            {
                damageCollider.enabled = true;
                damageCollider.isTrigger = true;
            }
            else
            {
                damageCollider.enabled = false;
                damageCollider.isTrigger = false;
            }

            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.8f)
            {
                GetComponentInChildren<AttackBoss>().ResetAttack();
            }
        }

        if (target == null)
        {
            Patrulha();
            return;
        }
        else
        {
            distanceToTarget = Vector3.Distance(agent.transform.position, target.position);
            agent.speed = 4.0f;
            IsPatrolling = false;
            targetBool = true;

            if (distanceToTarget > AttackDistance)
            {
                agent.isStopped = false;
                agent.destination = target.position;
                IsMoving = true;
                isAttacking = false;
            }
            else
            {
                agent.isStopped = true;
                IsMoving = false;
                isAttacking = true;
            }

            if (canMove)
            {
                animator.applyRootMotion = false;
                agent.isStopped = false;
            }
            else
            {
                animator.applyRootMotion = true;
                agent.isStopped = true;
            }
        }
    }

    public void Patrulha()
    {
        if (PatrulhaTargets.Length == 0) return;

        agent.speed = 2.0f;
        agent.isStopped = false;
        agent.destination = PatrulhaTargets[currentPatrolIndex].position;

        targetBool = false;
        IsPatrolling = true;
        canMove = true;
        IsMoving = true;
        isAttacking = false;
        animator.applyRootMotion = false;

        if (Vector3.Distance(agent.transform.position, PatrulhaTargets[currentPatrolIndex].position) < 0.7f)
        {
            currentPatrolIndex++;
            if (currentPatrolIndex >= PatrulhaTargets.Length)
            {
                currentPatrolIndex = 0;
            }
        }

        Debug.Log("Patrulha indo para o index " + currentPatrolIndex);
    }
}
