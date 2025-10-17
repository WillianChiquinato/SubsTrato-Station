using Fusion;
using UnityEngine;

public class CharacterMultiplayer : NetworkBehaviour
{
    [Header("Sounds")]
    public AudioSource PassosSound;
    public bool isDancinNow = false;

    [Header("Movement")]
    public float moveSpeed;
    private Vector3 moveDirection;

    public CharacterController character;
    public CapsuleCollider capsuleColliderCharacter;
    public Animator animator;
    public bool isMoving;

    public int selectedSkinIndex = 0;

    [Header("Gravidade player")]
    public Vector3 velocity;
    private float lastGroundedTime;
    private float coyoteTime = 0.1f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("GroundCheck")]
    public bool isGrounded;

    [Header("Knockback")]
    public float knockbackDuration = 0.3f;
    private float knockbackTimer;
    private Vector3 knockbackVelocity;
    [HideInInspector] public Health health;

    public bool canMove
    {
        get { return animator.GetBool("canMove"); }
        set { animator.SetBool("canMove", value); }
    }

    void Start()
    {
        character = GetComponent<CharacterController>();
        capsuleColliderCharacter = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!GetInput(out NetworkInputData inputData)) return;
        if (!Object.HasStateAuthority) return;
        if (character == null) return;

        isGrounded = character.isGrounded;
        if (character.isGrounded)
        {
            lastGroundedTime = Runner.SimulationTime;
        }

        bool isActuallyGrounded = Runner.SimulationTime - lastGroundedTime < coyoteTime;
        animator.SetBool("IsGround", isGrounded);
        animator.SetBool("Jump", isGrounded);
        animator.SetFloat("yVelocity", velocity.y);
        animator.SetBool("IsAlive", health.isAlive);
        animator.SetBool("isDancinNow", isDancinNow);

        if (!health.isAlive)
        {
            canMove = false;
            animator.SetTrigger("Death");
            character.Move(Vector3.zero);
            velocity = Vector3.zero;
            return;
        }

        if (!isGrounded)
        {
            animator.applyRootMotion = false;
        }

        // Knockback
        if (knockbackTimer > 0)
        {
            character.Move(knockbackVelocity * Runner.DeltaTime);
            knockbackTimer -= Runner.DeltaTime;
        }

        if (canMove)
        {
            animator.applyRootMotion = false;

            HandleMovement(inputData);
            HandleRotation(inputData);

            HandleAnimations(inputData);
            HandleDancin(inputData);

            if (isActuallyGrounded && inputData.jumpPressed)
            {
                Jump();
            }

            if (isActuallyGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            velocity.y += gravity * Runner.DeltaTime;
        }
        else
        {
            animator.applyRootMotion = true;
        }

        Debug.LogWarning("Console " + animator.applyRootMotion);
    }

    void OnAnimatorMove()
    {
        if (animator.applyRootMotion)
        {
            // pega o deslocamento da animação
            Vector3 rootMotion = animator.deltaPosition;

            // mantém o controle da gravidade
            rootMotion.y = velocity.y * Runner.DeltaTime;

            character.Move(rootMotion);
        }
    }

    private void HandleMovement(NetworkInputData inputData)
    {
        Vector3 direction = new Vector3(inputData.moveInput.x, 0f, inputData.moveInput.y);
        moveDirection = direction.normalized;

        Vector3 move = moveDirection * moveSpeed;
        move.y = velocity.y;

        character.Move(move * Runner.DeltaTime);
    }

    private void HandleRotation(NetworkInputData inputData)
    {
        Vector3 direction = new Vector3(inputData.moveInput.x, 0f, inputData.moveInput.y);

        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Runner.DeltaTime * 15f);
        }
    }

    private void HandleAnimations(NetworkInputData inputData)
    {
        if (!Object.HasInputAuthority) return;

        isMoving = inputData.moveInput.magnitude > 0.1f;
        animator.SetBool("Run", isMoving);

        if (isMoving && isGrounded && !animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            if (!PassosSound.isPlaying) PassosSound.Play();
        }
        else if (PassosSound.isPlaying)
        {
            PassosSound.Stop();
        }

        animator.SetFloat("horizontal", inputData.moveInput.x);
        animator.SetFloat("vertical", inputData.moveInput.y);
    }


    private void HandleDancin(NetworkInputData inputData)
    {
        isDancinNow = inputData.dancinHipHop || inputData.dancinSalsa || inputData.dancinSwing;

        int danceHipHop = inputData.dancinHipHop ? 1 : 0;
        int danceSalsa = inputData.dancinSalsa ? 2 : 0;
        int danceSwing = inputData.dancinSwing ? 3 : 0;

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("HipHop") ||
            animator.GetCurrentAnimatorStateInfo(0).IsName("SalsaDance") ||
            animator.GetCurrentAnimatorStateInfo(0).IsName("SwingDance"))
        {
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.97f)
            {
                isDancinNow = false;
                animator.applyRootMotion = false;
                animator.SetInteger("DanceNumber", 0);
            }
        }

        if (danceHipHop != 0)
        {
            animator.SetInteger("DanceNumber", danceHipHop);
        }
        else if (danceSalsa != 0)
        {
            animator.SetInteger("DanceNumber", danceSalsa);
        }
        else if (danceSwing != 0)
        {
            animator.SetInteger("DanceNumber", danceSwing);
        }
        else
        {
            animator.SetInteger("DanceNumber", 0);
        }
    }

    public void Jump()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    public void Attack()
    {
        animator.SetTrigger("Attack");
        canMove = false;
        moveDirection = Vector3.zero;
        Invoke("ResetAttack", 1.5f);
    }

    private void ResetAttack()
    {
        animator.applyRootMotion = false;
        canMove = true;
    }

    public void RpcUpdateSkin(int skinIndex)
    {
        selectedSkinIndex = skinIndex;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.name.StartsWith("Skin"))
            {
                child.gameObject.SetActive(false);
            }
        }

        Transform selectedSkin = transform.Find("Skin" + (skinIndex + 1));
        if (selectedSkin != null)
        {
            selectedSkin.gameObject.SetActive(true);
        }
    }
}

