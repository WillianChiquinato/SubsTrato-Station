using Fusion;
using UnityEngine;

public class PlayerMoviment : NetworkBehaviour
{
    public NetworkObject networkObject;

    [Header("Movement")]
    public float moveSpeed;
    public Transform orientation;
    public bool isStealth = false;
    public bool aimActive = false;
    public bool aimAnimActive = false;

    Vector3 moveDirection;
    float horizontalInput;
    float verticalInput;

    CharacterController character;
    public CapsuleCollider capsuleColliderCharacter;
    public Animator animator;
    public bool isMoving;
    public GameObject DeathUI;

    [Header("Gravidade player")]
    public Vector3 velocity;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("JumpKey")]
    public float airControlMultiplier;


    [Header("GroundCheck")]
    public bool isGrounded;
    public Transform groundCheck;
    public float groundDistance = 0.4f;


    [Header("Pick up Itens")]
    [SerializeField] private LayerMask pickUpLayer;
    [SerializeField] private LayerMask interactLayer;

    [SerializeField]
    [Min(1)]
    private float pickUpDistance = 2f;
    public Transform playerCameraTransform;
    public GameObject pickUpUI;
    private RaycastHit hit;

    public GameObject myHandItem;
    public GameObject ItemFlutuante;
    public float itemFlutuanteDistance = 2f;
    public float itemFlutuanteSpeed = 10f;
    public bool isPickingUp = false;

    [Header("Knockback")]
    public float knockbackDuration = 0.3f;
    private float knockbackTimer;
    private Vector3 knockbackVelocity;

    public bool Arremessar = false;

    [Header("Health e HealthBar")]
    [HideInInspector] public Health health;
    [HideInInspector] public EstaminaBar estaminaBar;
    public float estamina = 50f;

    public PlayerInventory inventory;

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

    void Awake()
    {
        networkObject = GetComponent<NetworkObject>();

        if (networkObject == null)
        {
            Debug.LogError("NetworkObject não encontrado no Awake!");
        }
        else
        {
            Debug.Log($"NetworkObject encontrado no Awake: {networkObject.Id}");
        }

        inventory = GetComponent<PlayerInventory>();
    }

    void Start()
    {
        character = GetComponent<CharacterController>();
        capsuleColliderCharacter = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
        estaminaBar = GetComponent<EstaminaBar>();

        animator.SetBool("StartGame", true);

        if (!networkObject.HasInputAuthority)
        {
            // Desativa câmera e componentes locais
            playerCameraTransform.gameObject.SetActive(false);
            return;
        }

        //References UI.
        var ui = UIreferences.Instance;
        DeathUI = ui.DeathUI;
        pickUpUI = ui.PickUpItemUI;
        DeathUI.SetActive(false);
        pickUpUI.SetActive(false);
    }

    public override void Spawned()
    {
        Debug.Log($"=== SPAWNED CHAMADO ===");
        Debug.Log($"GameObject: {gameObject.name}");
        Debug.Log($"NetworkObject: {networkObject != null}");
        Debug.Log($"HasInputAuthority: {networkObject?.HasInputAuthority}");
        Debug.Log($"Runner: {Runner != null}");
        Debug.Log($"Runner name: {(Runner != null ? Runner.name : "NULL")}");
        Debug.Log($"Object: {Object != null}");
        Debug.Log($"Object name: {(Object != null ? Object.name : "NULL")}");

        if (networkObject != null && networkObject.HasInputAuthority)
        {
            Debug.Log("Configurando para jogador local");
            UIreferences.Instance.player = gameObject;
        }

        if (Runner == null)
        {
            Debug.LogError("Runner ainda é null após Spawned()! Isso é um problema grave.");

            // Tentar encontrar o Runner manualmente
            NetworkRunner foundRunner = FindFirstObjectByType<NetworkRunner>();
            if (foundRunner != null)
            {
                Debug.Log($"Runner encontrado manualmente: {foundRunner.name}");
            }
            else
            {
                Debug.LogError("Nenhum NetworkRunner encontrado na cena!");
            }
        }
        else
        {
            Debug.Log("Runner encontrado: " + Runner.name);
        }
    }


    public override void FixedUpdateNetwork()
    {
        if (!networkObject.HasInputAuthority) return;
        if (!GetInput(out NetworkInputData inputData)) return;

        isGrounded = character.isGrounded;
        animator.SetBool("IsGround", isGrounded);
        animator.SetBool("Jump", isGrounded);
        animator.SetFloat("yVelocity", velocity.y);
        animator.SetBool("IsAlive", health.isAlive);
        animator.SetBool("AimPistol", aimAnimActive);
        animator.SetBool("Throw", Arremessar);

        if (health.isAlive)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("StartGame") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.05f)
            {
                animator.applyRootMotion = true;
                canMove = false;
                if (animator.GetCurrentAnimatorStateInfo(0).IsName("StartGame") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f)
                {
                    animator.SetBool("StartGame", false);
                    canMove = true;
                    animator.applyRootMotion = false;
                }
            }

            if (QuestSystem.instance.questArea)
            {
                if (QuestSystem.instance.isQuestAtivo)
                {
                    canMove = false;

                    // Liberar movimento do mouse
                    Cursor.lockState = CursorLockMode.Confined;
                    Cursor.visible = true;
                }
                else
                {
                    QuestSystem.instance.questArea = false;
                    QuestSystem.instance.isQuestAtivo = false;
                    canMove = true;

                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }


            if (isMoving)
            {
                if (estamina > 0 && !isStealth)
                {
                    estamina -= 1.6f * Time.fixedDeltaTime;
                }
                else if (isStealth)
                {
                    estamina += 1f * Time.fixedDeltaTime;
                }
            }
            else
            {
                if (estamina < 50f)
                {
                    estamina += 2f * Time.fixedDeltaTime;
                }
            }

            if (estamina <= 0)
            {
                moveSpeed = 3f;
            }
            else
            {
                moveSpeed = 6f;
            }

            if (knockbackTimer > 0)
            {
                character.Move(knockbackVelocity * Time.fixedDeltaTime);
                knockbackTimer -= Time.fixedDeltaTime;
            }

            if (canMove)
            {
                MyInput();
                DropItem();
                UseItem();

                if (inputData.jumpPressed && isGrounded && !isStealth && !aimActive)
                {
                    Jump();
                }

                Debug.DrawRay(playerCameraTransform.position, playerCameraTransform.forward * pickUpDistance, Color.red);
                //Pickable items
                if (hit.collider != null)
                {
                    hit.collider.GetComponent<HightLights>()?.ToggleHighlight(false);
                    pickUpUI.SetActive(false);
                }

                if (ItemFlutuante != null)
                {
                    Vector3 mousePos = Input.mousePosition;
                    mousePos.z = itemFlutuanteDistance; // distância da câmera

                    Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
                    ItemFlutuante.transform.position = Vector3.Lerp(
                        ItemFlutuante.transform.position,
                        worldPos,
                        Time.fixedDeltaTime * itemFlutuanteSpeed
                    );
                }

                //Sempre por ultimo.
                if (myHandItem != null)
                {
                    // Movement and gravity still need to be applied even if holding an item
                    MovePlayer();

                    if (isGrounded && velocity.y < 0)
                    {
                        velocity.y = -2f;
                    }

                    velocity.y += gravity * Time.fixedDeltaTime;
                    Vector2 moveInput = new Vector2(horizontalInput, verticalInput).normalized;
                    Vector3 finalMove = new Vector3(moveInput.x, 0, moveInput.y) * moveSpeed;

                    if (!isGrounded)
                    {
                        finalMove *= airControlMultiplier;
                    }

                    finalMove.y = velocity.y;
                    character.Move(finalMove * Time.fixedDeltaTime);

                    return;
                }

                CheckPickUp();
            }
            else
            {
                isMoving = false;
                horizontalInput = 0;
                verticalInput = 0;
            }

            // Movement and gravity
            MovePlayer();

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            velocity.y += gravity * Time.fixedDeltaTime;

            Vector3 move = moveDirection * moveSpeed;

            if (!isGrounded)
            {
                move *= airControlMultiplier;
            }

            move.y = velocity.y;
            character.Move(move * Time.fixedDeltaTime);
        }
        else
        {
            pickUpUI.SetActive(false);
            canMove = false;
            animator.SetTrigger("Death");
            DeathUI.SetActive(true);

            character.Move(Vector3.zero);
            velocity = Vector3.zero;
        }
    }

    private void MyInput()
    {
        if (!GetInput(out NetworkInputData inputData)) return;

        horizontalInput = inputData.moveInput.x;
        verticalInput = inputData.moveInput.y;
        isStealth = inputData.stealthPressed;
        if (isStealth)
        {
            horizontalInput *= 0.4f;
            verticalInput *= 0.4f;
            float targetHeight = isStealth ? 2.1f : 2.763223f;
            if (Mathf.Abs(capsuleColliderCharacter.height - targetHeight) > 0.01f)
            {
                capsuleColliderCharacter.height = Mathf.Lerp(capsuleColliderCharacter.height, targetHeight, Time.deltaTime * 10f);
            }

            float stealthCharacterHeight = Mathf.Lerp(character.height, 2.1f, Time.fixedDeltaTime * 10f);
            character.height = stealthCharacterHeight;
        }
        else
        {
            float normalColliderHeight = Mathf.Lerp(capsuleColliderCharacter.height, 2.763223f, Time.fixedDeltaTime * 10f);
            float normalCharacterHeight = Mathf.Lerp(character.height, 2.763223f, Time.fixedDeltaTime * 10f);
            capsuleColliderCharacter.height = normalColliderHeight;
            character.height = normalCharacterHeight;
        }

        isMoving = horizontalInput != 0 || verticalInput != 0;
        animator.SetBool("Run", isMoving);

        animator.SetFloat("horizontal", horizontalInput);
        animator.SetFloat("vertical", verticalInput);
        animator.SetBool("IsStealth", isStealth);
    }

    private void MovePlayer()
    {
        Vector3 forward = playerCameraTransform.forward;
        Vector3 right = playerCameraTransform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        moveDirection = forward * verticalInput + right * horizontalInput;
    }

    public void Jump()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    private void CheckPickUp()
    {
        if (!GetInput(out NetworkInputData inputData)) return;

        if (Physics.Raycast(playerCameraTransform.position, playerCameraTransform.forward, out hit, pickUpDistance, pickUpLayer))
        {
            HightLights highlight = hit.collider.GetComponent<HightLights>();
            if (highlight != null)
            {
                if (inventory.myHandItem == null)
                {
                    highlight.ToggleHighlight(true);
                    pickUpUI.SetActive(true);

                    if (inputData.interactPressed)
                    {
                        if (hit.collider.CompareTag("Quest"))
                        {
                            hit.collider.GetComponent<QuestTrigger>().TriggerQuest();
                        }
                        else
                        {
                            StartPickUp();
                        }
                    }
                }
            }
        }
        else if (Physics.Raycast(playerCameraTransform.position, playerCameraTransform.forward, out hit, pickUpDistance, interactLayer))
        {
            ChipSystem chip = hit.collider.GetComponent<ChipSystem>();
            HightLights highlight = hit.collider.GetComponent<HightLights>();
            if (chip != null && highlight != null)
            {
                if (inventory.myHandItem != null && inventory.myHandItem.GetComponent<Chip>())
                {
                    highlight.ToggleHighlight(true);
                    pickUpUI.SetActive(true);

                    if (inputData.interactPressed)
                    {
                        chip.Interact();
                        Destroy(inventory.myHandItem);
                        inventory.hotbarItems[inventory.selectedSlot] = null;
                        inventory.myHandItem = null;
                        inventory.UpdateHotbarUI();
                    }
                }
            }
        }
    }

    void StartPickUp()
    {
        if (!networkObject.HasInputAuthority) return;

        if (hit.collider != null && myHandItem == null && !isPickingUp)
        {
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            Debug.Log("Interacting with item: " + hit.collider.name);

            if (hit.collider.GetComponent<Food>() || hit.collider.GetComponent<Weapon>())
            {
                animator.SetTrigger("PickUp");
                canMove = false;
                isPickingUp = true;
                myHandItem = hit.collider.gameObject;

                // Chama o pickup de fato depois de 0.7 segundos
                if (animator.GetCurrentAnimatorStateInfo(0).IsName("PickCrounch"))
                {
                    Debug.Log("Animação com crounch");
                    Invoke(nameof(PickUp), 0.7f);
                }
                else
                {
                    Debug.Log("Animação sem crounch");
                    Invoke(nameof(PickUp), 0.7f);
                }

                pickUpUI.SetActive(false);
            }
            else if (hit.collider.GetComponent<Chip>())
            {
                Debug.Log("É um item do cenário");
                ItemFlutuante = hit.collider.gameObject;
                myHandItem = ItemFlutuante;

                if (rb != null)
                {
                    Destroy(rb);
                }

                ItemFlutuante.transform.SetParent(null);
                pickUpUI.SetActive(false);
            }
        }
    }

    void PickUp()
    {
        if (!networkObject.HasInputAuthority) return;
        if (myHandItem == null) return;

        isPickingUp = false;
        canMove = true;

        if (inventory == null)
        {
            Debug.LogWarning("PlayerInventory não encontrado no objeto.");
            return;
        }

        ItemClass itemComponent = myHandItem.GetComponent<ItemClass>();
        if (itemComponent == null)
        {
            Debug.LogWarning("ItemClass não encontrado no myHandItem.");
            return;
        }

        // Tenta adicionar o item ao primeiro slot vazio
        for (int i = 0; i < inventory.totalSlots; i++)
        {
            if (inventory.hotbarItems[i] == null)
            {
                inventory.hotbarItems[i] = itemComponent.itemSO;
                inventory.selectedSlot = i;
                inventory.UpdateHotbarUI();
                Destroy(myHandItem);

                Debug.Log("Item adicionado ao slot " + i + ": " + itemComponent.itemSO.name);
                break;
            }
        }

        Debug.Log("Item pego com sucesso!");
    }

    public void DropItem()
    {
        if (!GetInput(out NetworkInputData inputData)) return;
        if (!networkObject.HasInputAuthority) return;
        if (!inputData.dropItemPressed) return;
        if (inventory.myHandItem == null) return;

        // Sempre usa RPC, independente de ser servidor ou cliente
        RPC_RequestDropItem();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestDropItem()
    {
        Vector3 spawnPos = playerCameraTransform.position + playerCameraTransform.transform.forward * 1.2f + playerCameraTransform.up * -0.2f;

        ItemClass itemClass = inventory.myHandItem.GetComponent<ItemClass>();
        if (itemClass == null) return;
        GameObject dropPrefab = ItemDatabase.GetPrefabForItem(itemClass.itemSO);
        if (dropPrefab == null) return;

        var droppedItem = Runner.Spawn(dropPrefab, Vector3.zero, Quaternion.identity, Object.InputAuthority);
        droppedItem.transform.position = spawnPos;

        // Se tiver Rigidbody, aplica força
        if (droppedItem.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.position = spawnPos;
            rb.AddForce(playerCameraTransform.transform.forward * 5f + playerCameraTransform.up * 4f, ForceMode.Impulse);
        }

        RPC_ClearInventory();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ClearInventory()
    {
        if (inventory.myHandItem != null)
        {
            Destroy(inventory.myHandItem);
        }

        inventory.hotbarItems[inventory.selectedSlot] = null;
        inventory.myHandItem = null;
        inventory.UpdateHotbarUI();
    }

    public void UseItem()
    {
        if (!GetInput(out NetworkInputData inputData)) return;
        if (!networkObject.HasInputAuthority) return;

        if (inventory.myHandItem != null)
        {
            HightLights hl = inventory.myHandItem.GetComponent<HightLights>();
            Destroy(hl);

            if (inventory.myHandItem.GetComponent<Food>() != null)
            {
                if (inputData.useItemPressed)
                {
                    Debug.Log("Usando item de comida: " + inventory.myHandItem.name);
                    IUsable usable = inventory.myHandItem.GetComponent<IUsable>();
                    if (usable != null)
                    {
                        usable.Use(this.gameObject);
                    }

                    inventory.hotbarItems[inventory.selectedSlot] = null;
                    inventory.UpdateHotbarUI();
                }
            }
            else
            {
                if (inventory.myHandItem.GetComponent<ItemArremessavel>())
                {
                    //Segurar botao direito.
                    if (inputData.prepareArremessoPressed)
                    {
                        Arremessar = true;
                    }

                    if (inputData.arremessarPressed)
                    {
                        Arremessar = false;
                    }
                }
                else
                {
                    if (!inventory.myHandItem.GetComponent<Chip>())
                    {
                        if (inputData.aimActive)
                        {
                            Invoke(nameof(ToggleAim), 0.5f);
                            aimAnimActive = !aimAnimActive;
                            if (aimAnimActive)
                            {
                                inventory.myHandItem.transform.localPosition = Vector3.zero + inventory.myHandItem.GetComponent<Weapon>().AimOffset;
                                inventory.myHandItem.transform.localRotation = inventory.myHandItem.GetComponent<Weapon>().AimOffsetRotation;
                            }
                            else
                            {
                                inventory.myHandItem.transform.localPosition = Vector3.zero + inventory.myHandItem.GetComponent<Weapon>().Offset;
                                inventory.myHandItem.transform.localRotation = inventory.myHandItem.GetComponent<Weapon>().OffsetRotation;
                            }
                        }

                        if (inputData.useItemPressed && aimActive)
                        {
                            if (isStealth && isMoving)
                            {
                                return;
                            }

                            IUsable usable = inventory.myHandItem.GetComponent<IUsable>();
                            if (usable != null)
                            {
                                usable.Use(this.gameObject);
                            }
                        }
                    }
                }
            }

        }
    }

    public void ToggleAim()
    {
        aimActive = !aimActive;
    }

    public void Heal(int amount)
    {
        if (health != null)
        {
            health.Heal(amount);
        }
    }

    public void BoostStamina(int amount, float duration)
    {
        if (estaminaBar != null)
        {
            estaminaBar.Boost(amount, duration);
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
}
