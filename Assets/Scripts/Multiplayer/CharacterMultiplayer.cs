using Fusion;
using UnityEngine;

public class CharacterMultiplayer : NetworkBehaviour
{
    [Header("Sounds")]
    public AudioSource PassosSound;
    public AudioSource HipHopSound;
    public AudioSource SalsaSound;
    public AudioSource SwingSound;
    public bool isDancinNow = false;
    public bool isRemaining = false;

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

        // Knockback
        if (knockbackTimer > 0)
        {
            character.Move(knockbackVelocity * Runner.DeltaTime);
            knockbackTimer -= Runner.DeltaTime;
        }

        HandleDancin(inputData);

        // Determina se o root motion deve estar ativo
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        bool isAttackOrDance = state.IsName("Attack") || state.IsName("HipHop") ||
                               state.IsName("SalsaDance") || state.IsName("SwingDance");

        // Root motion ativo apenas durante ataque/dança e se o personagem estiver vivo
        animator.applyRootMotion = isAttackOrDance && health.isAlive;

        if (canMove && !isAttackOrDance)
        {
            HandleMovement(inputData);
            HandleRotation(inputData);
            HandleAnimations(inputData);

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

        Debug.LogWarning("RootMotion ativo: " + animator.applyRootMotion);
    }

    void OnAnimatorMove()
    {
        if (animator.applyRootMotion)
        {
            Vector3 rootMotion = animator.deltaPosition;
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
        int danceHipHop = 0;
        int danceSalsa = 0;
        int danceSwing = 0;

        if (!isRemaining)
        {
            danceHipHop = inputData.dancinHipHop ? 1 : 0;
            danceSalsa = inputData.dancinSalsa ? 3 : 0;
            danceSwing = inputData.dancinSwing ? 2 : 0;
        }

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if (state.IsName("HipHop") || state.IsName("SalsaDance") || state.IsName("SwingDance"))
        {
            isRemaining = true;
            if (state.normalizedTime >= 0.9f)
            {
                danceHipHop = danceSalsa = danceSwing = 0;
                isDancinNow = false;
                animator.applyRootMotion = false;
                animator.SetInteger("DanceNumber", 0);
            }

            if (state.normalizedTime >= 0.96f)
            {
                HipHopSound.Stop();
                SalsaSound.Stop();
                SwingSound.Stop();
                isRemaining = false;
            }
        }

        if (danceHipHop != 0)
        {
            animator.SetInteger("DanceNumber", danceHipHop);
            if (!isRemaining) HipHopSound.Play();
        }
        else if (danceSalsa != 0)
        {
            animator.SetInteger("DanceNumber", danceSalsa);
            if (!isRemaining) SalsaSound.Play();
        }
        else if (danceSwing != 0)
        {
            animator.SetInteger("DanceNumber", danceSwing);
            if (!isRemaining) SwingSound.Play();
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
    }

    // Chamado no fim da animação de ataque (adicione Animation Event)
    public void OnAttackAnimationEnd()
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

