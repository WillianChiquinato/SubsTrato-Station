using UnityEngine;

public class PlayerMoviment : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    CharacterController character;
    public Animator animator;
    public bool isMoving;

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
    [SerializeField]
    [Min(1)]
    private float pickUpDistance = 2f;
    public Transform playerCameraTransform;
    public GameObject pickUpUI;
    private RaycastHit hit;

    public Transform pickUpParent;
    public GameObject myHandItem;
    public GameObject ItemFlutuante;
    public float itemFlutuanteDistance = 2f;
    public float itemFlutuanteSpeed = 10f;
    public bool isPickingUp = false;

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
        pickUpUI.SetActive(false);

        animator = GetComponent<Animator>();
    }

    void Update()
    {
        isGrounded = character.isGrounded;
        animator.SetBool("IsGround", isGrounded);
        animator.SetBool("Jump", isGrounded);
        animator.SetFloat("yVelocity", velocity.y);
        if (canMove)
        {
            MyInput();
            DropItem();
            UseItem();

            if (Input.GetKey(jumpKey) && isGrounded)
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
                return;
            }

            if (Physics.Raycast(playerCameraTransform.position, playerCameraTransform.forward, out hit, pickUpDistance, pickUpLayer))
            {
                hit.collider.GetComponent<HightLights>()?.ToggleHighlight(true);
                pickUpUI.SetActive(true);
                if (Input.GetKeyDown(KeyCode.E))
                {
                    StartPickUp();
                }
            }
        }
    }

    void FixedUpdate()
    {
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

    private void movePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
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

            if (hit.collider.GetComponent<Food>() || hit.collider.GetComponent<Weapon>())
            {
                animator.SetTrigger("PickUp");
                canMove = false;
                isPickingUp = true;
                myHandItem = hit.collider.gameObject;

                // Chama o pickup de fato depois de 0.7 segundos
                Invoke(nameof(PickUp), 0.7f);
            }
            else if (hit.collider.GetComponent<Item>())
            {
                Debug.Log("É um item do cenário");
                ItemFlutuante = hit.collider.gameObject;
                myHandItem = ItemFlutuante;

                if (rb != null)
                    Destroy(rb);

                ItemFlutuante.transform.SetParent(null);
            }
        }
    }

    void PickUp()
    {
        if (myHandItem == null) return;

        Rigidbody rb = myHandItem.GetComponent<Rigidbody>();
        if (rb != null)
            Destroy(rb);

        myHandItem.transform.SetParent(pickUpParent, false);
        myHandItem.transform.localPosition = Vector3.zero;
        myHandItem.transform.localRotation = Quaternion.identity;

        isPickingUp = false;
        canMove = true;

        Debug.Log("Item pego com sucesso!");
    }

    public void DropItem()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (myHandItem != null)
            {
                myHandItem.AddComponent<Rigidbody>();
                Rigidbody rb = myHandItem.GetComponent<Rigidbody>();

                myHandItem.transform.SetParent(null);
                ItemFlutuante = null;
                if (myHandItem.GetComponent<Item>())
                {
                    Debug.Log("É um item do cenário, sem jogar item!!");
                }
                else
                {
                    rb.AddForce(playerCameraTransform.forward * 2f, ForceMode.Impulse);
                    rb.AddForce(playerCameraTransform.up * 4f, ForceMode.Impulse);
                }
                myHandItem = null;
            }
        }
    }

    public void UseItem()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (myHandItem != null)
            {
                IUsable usable = myHandItem.GetComponent<IUsable>();
                if (usable != null)
                {
                    usable.Use(this.gameObject);
                }
            }
        }
    }
}
