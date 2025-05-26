using UnityEngine;
using UnityEngine.AI;

public class AiTarget : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public bool targetBool;
    public float AttackDistance;

    private NavMeshAgent agent;
    private Animator animator;
    private float distanceToTarget;

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

        //Debug por enquanto.
        // target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        animator.SetBool("isMoving", IsMoving);
        animator.SetBool("isAttacking", isAttacking);
        animator.SetBool("Patrulha", IsPatrolling);
        animator.SetBool("Target", targetBool);

        if (target == null)
        {
            IsPatrolling = true;
            targetBool = false;
            agent.speed = 2.0f;
            Patrulha();
        }
        else
        {
            agent.speed = 4.0f;
            targetBool = true;
            IsPatrolling = false;
            distanceToTarget = Vector3.Distance(agent.transform.position, target.position);

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
        }

        if (!IsMoving)
        {
            canMove = false;
        }
        else
        {
            canMove = true;
        }
    }

    public void Patrulha()
    {
        if (PatrulhaTargets.Length == 0) return;

        agent.isStopped = false;
        agent.destination = PatrulhaTargets[currentPatrolIndex].position;
        IsMoving = true;
        isAttacking = false;

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
