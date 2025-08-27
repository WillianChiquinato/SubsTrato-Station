using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMoviment : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public Transform orientation;
    public bool isStealth = false;
    public bool aimActive = false;
    public bool aimAnimActive = false;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
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
    public KeyCode jumpKey = KeyCode.Space;
    public float airControlMultiplier;


    [Header("GroundCheck")]
    public bool isGrounded;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;


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
        DeathUI.SetActive(false);
        inventory = GetComponent<PlayerInventory>();
    }

    void Start()
    {
        character = GetComponent<CharacterController>();
        capsuleColliderCharacter = GetComponent<CapsuleCollider>();
        pickUpUI.SetActive(false);

        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
        estaminaBar = GetComponent<EstaminaBar>();

        animator.SetBool("StartGame", true);
    }

    public void Update()
    {
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

            if (QuestSystem.instance != null && QuestSystem.instance.questArea)
            {
                if (QuestSystem.instance.isQuestAtivo)
                {
                    canMove = false;
                    //Liberar movimento do mouse.
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    canMove = true;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }

            if (isMoving)
            {
                if (estamina > 0 && !isStealth)
                {
                    estamina -= 2f * Time.deltaTime;
                }
                else if (isStealth)
                {
                    estamina += 1f * Time.deltaTime;
                }
            }
            else
            {
                if (estamina < 50f)
                {
                    estamina += 2f * Time.deltaTime;
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
                character.Move(knockbackVelocity * Time.deltaTime);
                knockbackTimer -= Time.deltaTime;
            }

            if (canMove)
            {
                MyInput();
                DropItem();
                UseItem();

                if (Input.GetKey(jumpKey) && isGrounded && !isStealth && !aimActive)
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
                        Time.deltaTime * itemFlutuanteSpeed
                    );
                }

                //Sempre por ultimo.
                if (myHandItem != null)
                {
                    // Movement and gravity still need to be applied even if holding an item
                    movePlayer();

                    if (isGrounded && velocity.y < 0)
                    {
                        velocity.y = -2f;
                    }

                    velocity.y += gravity * Time.deltaTime;

                    Vector3 finalMove = moveDirection * moveSpeed;

                    if (!isGrounded)
                        finalMove *= airControlMultiplier;

                    finalMove.y = velocity.y;
                    character.Move(finalMove * Time.deltaTime);

                    return;
                }

                if (Physics.Raycast(playerCameraTransform.position, playerCameraTransform.forward, out hit, pickUpDistance, pickUpLayer))
                {
                    HightLights highlight = hit.collider.GetComponent<HightLights>();
                    if (highlight != null)
                    {
                        if (inventory.myHandItem == null)
                        {
                            highlight.ToggleHighlight(true);
                            pickUpUI.SetActive(true);

                            if (Input.GetKeyDown(KeyCode.E))
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

                            if (Input.GetKeyDown(KeyCode.E))
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
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                isMoving = false;
                horizontalInput = 0;
                verticalInput = 0;
            }

            // Movement and gravity
                movePlayer();

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            velocity.y += gravity * Time.deltaTime;

            Vector3 move = moveDirection * moveSpeed;

            if (!isGrounded)
            {
                move *= airControlMultiplier;
            }

            move.y = velocity.y;
            character.Move(move * Time.deltaTime);
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
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        isStealth = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        if (isStealth)
        {
            horizontalInput *= 0.4f;
            verticalInput *= 0.4f;
            float stealthColliderHeight = Mathf.Lerp(capsuleColliderCharacter.height, 2.1f, Time.deltaTime * 10f);
            float stealthCharacterHeight = Mathf.Lerp(character.height, 2.1f, Time.deltaTime * 10f);
            capsuleColliderCharacter.height = stealthColliderHeight;
            character.height = stealthCharacterHeight;
        }
        else
        {
            float normalColliderHeight = Mathf.Lerp(capsuleColliderCharacter.height, 2.763223f, Time.deltaTime * 10f);
            float normalCharacterHeight = Mathf.Lerp(character.height, 2.763223f, Time.deltaTime * 10f);
            capsuleColliderCharacter.height = normalColliderHeight;
            character.height = normalCharacterHeight;
        }

        isMoving = horizontalInput != 0 || verticalInput != 0;
        animator.SetBool("Run", isMoving);

        animator.SetFloat("horizontal", horizontalInput);
        animator.SetFloat("vertical", verticalInput);
        animator.SetBool("IsStealth", isStealth);
    }

    private void movePlayer()
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

    void StartPickUp()
    {
        if (hit.collider != null && myHandItem == null && !isPickingUp)
        {
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            Debug.Log("Interacting with item: " + hit.collider.name);

            if (hit.collider.GetComponent<Food>() || hit.collider.GetComponent<Weapon>() || hit.collider.GetComponent<Chip>())
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
            else if (hit.collider.GetComponent<Item>())
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
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ItemFlutuante = null;
            if (myHandItem != null)
            {
                myHandItem.AddComponent<Rigidbody>();
                myHandItem = null;
            }

            if (inventory.myHandItem != null)
            {
                inventory.myHandItem.AddComponent<Rigidbody>();
                inventory.myHandItem.AddComponent<HightLights>();

                var render1 = inventory.myHandItem.GetComponent<HightLights>().renderers = new List<Renderer>(inventory.myHandItem.GetComponents<Renderer>());
                if (render1 == null)
                {
                    inventory.myHandItem.GetComponent<HightLights>().renderers = new List<Renderer>(inventory.myHandItem.GetComponentsInChildren<Renderer>());
                }
                Rigidbody rb = inventory.myHandItem.GetComponent<Rigidbody>();

                inventory.myHandItem.transform.SetParent(null);
                if (inventory.myHandItem.GetComponent<Item>())
                {
                    Debug.Log("É um item do cenário, sem jogar item!!");
                }
                else
                {
                    aimAnimActive = false;
                    aimActive = false;
                    orientation.GetComponent<PlayerCam>().rightArmIK.weight = 0;
                    orientation.GetComponent<PlayerCam>().leftArmIK.weight = 0;

                    //quando dropar o item, tira o slot do item selecionado.
                    if (inventory != null)
                    {
                        rb.AddForce(playerCameraTransform.forward * 2f, ForceMode.Impulse);
                        rb.AddForce(playerCameraTransform.up * 4f, ForceMode.Impulse);
                        inventory.hotbarItems[inventory.selectedSlot] = null;
                        inventory.justDroppedItem = true;
                        inventory.UpdateHotbarUI();
                    }
                    else
                    {
                        Debug.LogWarning("PlayerInventory não encontrado no objeto.");
                    }
                }
                inventory.myHandItem = null;
            }
        }
    }

    public void UseItem()
    {
        if (inventory.myHandItem != null)
        {
            HightLights hl = inventory.myHandItem.GetComponent<HightLights>();
            Destroy(hl);

            if (inventory.myHandItem.GetComponent<Food>() != null)
            {
                if (Input.GetMouseButtonDown(0))
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
                    if (Input.GetMouseButton(1))
                    {
                        Arremessar = true;
                    }

                    if (Input.GetMouseButtonUp(1))
                    {
                        Arremessar = false;
                        Destroy(inventory.myHandItem);

                        myHandItem = null;
                        inventory.myHandItem = null;
                        inventory.hotbarItems[inventory.selectedSlot] = null;
                        inventory.justDroppedItem = true;
                        inventory.UpdateHotbarUI();
                        Debug.Log("Item arremessável destruído.");
                    }
                }
                else
                {
                    if (!inventory.myHandItem.GetComponent<Chip>())
                    {
                        if (Input.GetMouseButtonDown(1))
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

                        if (Input.GetMouseButtonDown(0) && aimActive)
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
