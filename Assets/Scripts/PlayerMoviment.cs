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

    void Start()
    {
        character = GetComponent<CharacterController>();
    }

    void Update()
    {
        isGrounded = character.isGrounded;
        MyInput();

        Debug.DrawRay(playerCameraTransform.position, playerCameraTransform.forward * pickUpDistance, Color.red);
        //Pickable items
        if(hit.collider != null)
        {
            hit.collider.GetComponent<HightLights>()?.ToggleHighlight(false);
            pickUpUI.SetActive(false);
        }
        if(Physics.Raycast(playerCameraTransform.position, playerCameraTransform.forward, out hit, pickUpDistance, pickUpLayer))
        {
            hit.collider.GetComponent<HightLights>()?.ToggleHighlight(true);
            pickUpUI.SetActive(true);
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("Pegou o item");
                // hit.collider.GetComponent<PickableItem>().Pick();
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

        // Gravidade
        velocity.y += gravity * Time.deltaTime;
        character.Move(velocity * Time.deltaTime);
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        if (Input.GetKey(jumpKey) && isGrounded)
        {
            Jump();
        }
    }

    private void movePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        if (character.isGrounded)
        {
            character.Move(moveDirection * moveSpeed * Time.deltaTime);
        }
        else
        {
            character.Move(moveDirection * moveSpeed * Time.deltaTime * airControlMultiplier);
        }
    }

    public void Jump()
    {
        if (isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }
}
