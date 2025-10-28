using Fusion;
using UnityEngine;

public class CharacterMultiplayer : NetworkBehaviour
{
    public NetworkObject networkObject;

    [Header("Sounds")]
    public AudioSource PassosSound;
    public AudioSource HipHopSound;
    public AudioSource SalsaSound;
    public AudioSource SwingSound;

    [Networked] public bool isDancinNow { get; set; } = false;
    [Networked] public bool isRemaining { get; set; } = false;
    [Networked] public Vector3 velocity { get; set; }
    [Networked] public bool isGrounded { get; set; } = false;

    [Header("Movement")]
    public float moveSpeed = 5f;
    private Vector3 moveDirection;

    public CharacterController character;
    public CapsuleCollider capsuleColliderCharacter;
    public Animator animator;
    public bool isMoving;

    public int selectedSkinIndex = 0;

    [Header("Gravidade player")]
    private float lastGroundedTime;
    private float coyoteTime = 0.1f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

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

    void Awake()
    {
        networkObject = GetComponent<NetworkObject>();
        character = GetComponent<CharacterController>();
        capsuleColliderCharacter = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
    }

    void Start()
    {
        var ui = UIreferencesLobby.Instance;
        if (ui != null)
        {
            ui.MostrarBtn.SetActive(true);
            ui.ViewObjs.GetComponent<CanvasGroup>().alpha = 0;
            ui.ViewObjs.GetComponent<CanvasGroup>().interactable = false;
            ui.ViewObjs.GetComponent<CanvasGroup>().blocksRaycasts = false;
            ui.SelectedSkinsUI.SetActive(false);
        }
    }

    public override void Spawned()
    {
        Debug.Log($"Player spawned - HasInputAuthority: {Object.HasInputAuthority}, IsProxy: {Object.IsProxy}");

        if (Object.HasInputAuthority)
        {
            Debug.Log("Configurando para jogador local");
            if (UIreferencesLobby.Instance != null)
            {
                UIreferencesLobby.Instance.player = gameObject;
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Processa input apenas para o jogador local
        if (GetInput(out NetworkInputData inputData))
        {
            ProcessInput(inputData);
        }

        // Atualiza animações e estados para TODOS os jogadores
        UpdateCommonStates();
    }

    private void ProcessInput(NetworkInputData inputData)
    {
        if (!Object.HasInputAuthority) return;
        if (character == null) return;

        isGrounded = character.isGrounded;
        if (character.isGrounded)
        {
            lastGroundedTime = Runner.SimulationTime;
        }

        bool isActuallyGrounded = Runner.SimulationTime - lastGroundedTime < coyoteTime;

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
            return;
        }

        HandleDancin(inputData);

        // Determina se o root motion deve estar ativo
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        bool isAttackOrDance = state.IsName("Attack") || state.IsName("HipHop") ||
                               state.IsName("SalsaDance") || state.IsName("SwingDance");

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
                var vel = velocity;
                vel.y = -2f;
                velocity = vel;
            }

            var v = velocity;
            v.y += gravity * Runner.DeltaTime;
            velocity = v;
        }
    }

    private void UpdateCommonStates()
    {
        // Atualiza animações para TODOS os jogadores (local e remotos)
        animator.SetBool("IsGround", isGrounded);
        animator.SetBool("Jump", isGrounded);
        animator.SetFloat("yVelocity", velocity.y);
        animator.SetBool("IsAlive", health.isAlive);
        animator.SetBool("isDancinNow", isDancinNow);
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
        isMoving = inputData.moveInput.magnitude > 0.1f;
        animator.SetBool("Run", isMoving);

        // Sons apenas para jogador local
        if (Object.HasInputAuthority)
        {
            if (isMoving && isGrounded && !animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            {
                if (!PassosSound.isPlaying) PassosSound.Play();
            }
            else if (PassosSound.isPlaying)
            {
                PassosSound.Stop();
            }
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
            if (!isRemaining && Object.HasInputAuthority) HipHopSound.Play();
        }
        else if (danceSalsa != 0)
        {
            animator.SetInteger("DanceNumber", danceSalsa);
            if (!isRemaining && Object.HasInputAuthority) SalsaSound.Play();
        }
        else if (danceSwing != 0)
        {
            animator.SetInteger("DanceNumber", danceSwing);
            if (!isRemaining && Object.HasInputAuthority) SwingSound.Play();
        }
        else
        {
            animator.SetInteger("DanceNumber", 0);
        }
    }

    public void Jump()
    {
        var vel = velocity;
        vel.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        velocity = vel;
    }

    public void Attack()
    {
        animator.SetTrigger("Attack");
        canMove = false;
        moveDirection = Vector3.zero;
    }

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

