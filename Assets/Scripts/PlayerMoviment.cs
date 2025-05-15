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
            PickUp();
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
    }

    private void movePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
    }

    public void Jump()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

    }

    public void PickUp()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (hit.collider != null && myHandItem == null)
            {
                // Interact with the item
                Debug.Log("Interacting with item: " + hit.collider.name);
                Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
                if (hit.collider.GetComponent<Food>() || hit.collider.GetComponent<Weapon>())
                {
                    Debug.Log("Item de comida");
                    myHandItem = hit.collider.gameObject;
                    myHandItem.transform.SetParent(pickUpParent, false);

                    myHandItem.transform.localPosition = Vector3.zero;
                    myHandItem.transform.localRotation = Quaternion.identity;

                    if (rb != null)
                    {
                        rb.isKinematic = true;
                    }
                    return;
                }

                if (hit.collider.GetComponent<Item>())
                {
                    Debug.Log("É um item do cenário");

                    ItemFlutuante = hit.collider.gameObject;
                    myHandItem = ItemFlutuante;

                    if (rb != null)
                    {
                        rb.isKinematic = true;
                    }

                    ItemFlutuante.transform.SetParent(null);
                    return;
                }
            }
        }
    }

    public void DropItem()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (myHandItem != null)
            {
                myHandItem.transform.SetParent(null);
                ItemFlutuante = null;
                Rigidbody rb = myHandItem.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
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
