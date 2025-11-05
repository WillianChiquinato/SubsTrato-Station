using Fusion;
using UnityEngine;

public class CharacterMultiplayer : NetworkBehaviour
{
    public NetworkObject networkObject;
    private NetworkMecanimAnimator _networkAnimator;
    [Networked] public bool IsReady { get; set; }

    [Header("Sounds")]
    public AudioSource PassosSound;
    public AudioSource HipHopSound;
    public AudioSource SalsaSound;
    public AudioSource SwingSound;

    [Networked] public bool isDancinNow { get; set; } = false;
    [Networked] public bool isRemaining { get; set; } = false;
    [Networked] public Vector3 velocity { get; set; }
    [Networked] public bool isGrounded { get; set; } = false;
    [Networked] public NetworkButtons ButtonsPrevious { get; set; }
    [Networked] public Vector2 NetworkedMoveInput { get; set; }
    [Networked] public int NetworkedDanceNumber { get; set; }

    [Header("Movement")]
    public float moveSpeed = 5f;
    private Vector3 moveDirection;

    public CharacterController character;
    public CapsuleCollider capsuleColliderCharacter;
    public Animator animator;

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

        _networkAnimator = GetComponent<NetworkMecanimAnimator>();
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
        Debug.Log($"=== PLAYER SPAWNED ===");
        Debug.Log($"Object: {gameObject.name}");
        Debug.Log($"HasInputAuthority: {Object.HasInputAuthority}");
        Debug.Log($"IsProxy: {Object.IsProxy}");
        Debug.Log($"InputAuthority: {Object.InputAuthority}");
        Debug.Log($"Runner LocalPlayer: {Runner?.LocalPlayer}");

        if (Object.HasInputAuthority)
        {
            Debug.Log("🎮 Este é o jogador LOCAL - configurando controle");
            SetupLocalPlayer();
        }
        else
        {
            Debug.Log("👀 Este é um jogador REMOTO - desabilitando inputs locais");
            SetupRemotePlayer();
        }

        if (_networkAnimator != null)
        {
            _networkAnimator.Animator = animator;
        }

        ConfigureCommonSettings();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_NotifyReady()
    {
        IsReady = true;
        Debug.Log($"✅ Character {Object.InputAuthority} marcado como pronto");

        var gm = GameManagerMultiplayer.instance;
        if (gm == null)
        {
            gm = FindFirstObjectByType<GameManagerMultiplayer>();
        }

        if (gm != null && Runner.IsServer)
        {
            if (gm._playersReady.ContainsKey(Object.InputAuthority))
            {
                gm._playersReady[Object.InputAuthority] = true;
            }
            else
            {
                gm._playersReady.Add(Object.InputAuthority, true);
            }
        }
    }

    private void SetupLocalPlayer()
    {
        // Habilitar componentes específicos do jogador local
        if (UIreferencesLobby.Instance != null)
        {
            UIreferencesLobby.Instance.player = gameObject;
            Debug.Log("✅ Referência de UI configurada para jogador local");
        }

        AudioListener audioListener = GetComponent<AudioListener>();
        if (audioListener != null)
        {
            audioListener.enabled = true;
        }

        if (character != null)
        {
            character.enabled = true;
        }

        Debug.Log("✅ Jogador local configurado com sucesso");
    }

    private void SetupRemotePlayer()
    {
        AudioListener audioListener = GetComponent<AudioListener>();
        if (audioListener != null)
        {
            audioListener.enabled = false;
        }
    }

    private void ConfigureCommonSettings()
    {
        if (animator != null)
        {
            animator.applyRootMotion = false;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData inputData))
        {
            ProcessInput(inputData);
        }

        UpdateCommonStates();
    }

    private void ProcessInput(NetworkInputData inputData)
    {
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

        NetworkedMoveInput = inputData.moveInput;

        // CORREÇÃO: HandleDancin deve vir ANTES da verificação de canMove
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
        animator.SetBool("IsGround", isGrounded);
        animator.SetBool("Jump", isGrounded);
        animator.SetFloat("yVelocity", velocity.y);
        animator.SetBool("IsAlive", health.isAlive);
        animator.SetBool("isDancinNow", isDancinNow);

        animator.SetFloat("horizontal", NetworkedMoveInput.x);
        animator.SetFloat("vertical", NetworkedMoveInput.y);

        bool isMoving = NetworkedMoveInput.magnitude > 0.1f;
        animator.SetBool("Run", isMoving);
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
        
        Debug.Log($"Move: {move}, DeltaTime: {Runner.DeltaTime}");
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

    private void HandleDancin(NetworkInputData inputData)
    {
        // Apenas processar dança se o jogador estiver vivo e puder se mover
        if (!health.isAlive) return;

        bool anyDancePressed = inputData.dancinHipHop || inputData.dancinSalsa || inputData.dancinSwing;

        if (!isRemaining && anyDancePressed && Object.HasInputAuthority)
        {
            int danceNumber = 0;
            if (inputData.dancinHipHop) danceNumber = 1;
            else if (inputData.dancinSalsa) danceNumber = 3;
            else if (inputData.dancinSwing) danceNumber = 2;

            if (danceNumber > 0)
            {
                RPC_StartDance(danceNumber);
            }
        }

        // Atualizar animator com o número da dança atual
        animator.SetInteger("DanceNumber", NetworkedDanceNumber);

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if (state.IsName("HipHop") || state.IsName("SalsaDance") || state.IsName("SwingDance"))
        {
            if (state.normalizedTime >= 0.95f)
            {
                if (Object.HasStateAuthority)
                {
                    NetworkedDanceNumber = 0;
                    isDancinNow = false;
                    isRemaining = false;
                }
            }
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_StartDance(int danceNumber)
    {
        NetworkedDanceNumber = danceNumber;
        isDancinNow = true;
        isRemaining = true;

        Debug.Log($"💃 Dança iniciada: {danceNumber}");
    }

    public override void Render()
    {
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (Object.HasInputAuthority)
        {
            bool isMoving = NetworkedMoveInput.magnitude > 0.1f;

            if (isMoving && isGrounded && !isDancinNow && !animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            {
                if (!PassosSound.isPlaying) PassosSound.Play();
            }
            else if (PassosSound.isPlaying)
            {
                PassosSound.Stop();
            }

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Alpha3))
            {
                Debug.Log($"Tecla de dança pressionada - DanceNumber: {NetworkedDanceNumber}, isDancinNow: {isDancinNow}");
            }
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

