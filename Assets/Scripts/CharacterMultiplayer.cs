using UnityEngine;

public class CharacterMultiplayer : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    CharacterController character;
    public CapsuleCollider capsuleColliderCharacter;
    public Animator animator;
    public bool isMoving;

    [Header("Gravidade player")]
    public Vector3 velocity;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("JumpKey")]
    public KeyCode jumpKey = KeyCode.Space;
    public float airControlMultiplier;

    [Header("Attack")]
    public KeyCode attackKey = KeyCode.Mouse0;
    public CharacterMultiplayerAttack attackCollider;


    [Header("GroundCheck")]
    public bool isGrounded;


    [Header("Knockback")]
    public float knockbackDuration = 0.3f;
    private float knockbackTimer;
    private Vector3 knockbackVelocity;
    [HideInInspector] public Health health;

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
        character = GetComponent<CharacterController>();
        capsuleColliderCharacter = GetComponent<CapsuleCollider>();
        // attackCollider.GetComponentInChildren<CharacterMultiplayerAttack>();

        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
        attackCollider.GetComponent<SphereCollider>().enabled = false;
    }

    void Update()
    {
        isGrounded = character.isGrounded;
        animator.SetBool("IsGround", isGrounded);
        animator.SetBool("Jump", isGrounded);
        animator.SetFloat("yVelocity", velocity.y);
        animator.SetBool("IsAlive", health.isAlive);

        if (health.isAlive)
        {
            if (knockbackTimer > 0)
            {
                character.Move(knockbackVelocity * Time.deltaTime);
                knockbackTimer -= Time.deltaTime;
            }

            if (canMove)
            {
                MyInput();
                RotatePlayer();
            }

            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.4f)
            {
                attackCollider.GetComponent<SphereCollider>().enabled = true;
                if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
                {
                    attackCollider.GetComponent<SphereCollider>().enabled = false;
                    attackCollider.ResetAttack();
                }
            }
        }
        else
        {
            canMove = false;
            animator.SetTrigger("Death");
        }
    }

    void FixedUpdate()
    {
        if (health.isAlive)
        {
            movePlayer();

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            if (Input.GetKey(jumpKey) && isGrounded)
            {
                Jump();
            }

            if (Input.GetKey(attackKey) && canMove && isGrounded)
            {
                Attack();
            }

            velocity.y += gravity * Time.deltaTime;

            Vector3 finalMove = moveDirection * moveSpeed;

            if (!isGrounded)
            {
                finalMove *= airControlMultiplier;
            }

            finalMove.y = velocity.y;
            character.Move(finalMove * Time.deltaTime);
        }
        else
        {
            character.Move(Vector3.zero);
            velocity = Vector3.zero;
        }
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        isMoving = horizontalInput != 0 || verticalInput != 0;
        animator.SetBool("Run", isMoving);

        animator.SetFloat("horizontal", horizontalInput);
        animator.SetFloat("vertical", verticalInput);
    }

    private void RotatePlayer()
    {
        Vector3 direction = new Vector3(horizontalInput, 0f, verticalInput);

        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 15f);
        }
    }

    private void movePlayer()
    {
        if (!canMove)
        {
            moveDirection = Vector3.zero;
            return;
        }

        Vector3 inputDirection = new Vector3(horizontalInput, velocity.y, verticalInput).normalized;

        moveDirection = inputDirection * moveSpeed;

        // Aplica movimento com o CharacterController
        character.Move(moveDirection * Time.deltaTime);
    }

    public void Jump()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
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

    public void Attack()
    {
        animator.SetTrigger("Attack");
        canMove = false;
        moveDirection = Vector3.zero;
        animator.applyRootMotion = true;
        Invoke("ResetAttack", 1.5f);
    }

    private void ResetAttack()
    {
        animator.applyRootMotion = false;
        canMove = true;
    }
}
