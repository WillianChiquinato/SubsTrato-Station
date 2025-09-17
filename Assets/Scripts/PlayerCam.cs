using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerCam : MonoBehaviour
{
    [Header("Sensibilidade do mouse")]
    public float sensX = 100f;
    public float sensY = 100f;

    [Header("Referências")]
    public Transform playerBody;
    public PlayerMoviment player;
    public Transform cameraAnchor;
    public TwoBoneIKConstraint rightArmIK;
    public TwoBoneIKConstraint leftArmIK;

    [Header("Configuração de movimento")]
    public float followSmoothTime = 0.1f; 

    private float xRotation = 0f;
    private float lookX;
    private float lookY;
    private Vector3 camVelocity;
    private bool wasLocked = true;

    void Start()
    {
        player = playerBody.GetComponent<PlayerMoviment>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        rightArmIK.weight = 0;
        leftArmIK.weight = 0;
    }

    void Update()
    {
        // Leitura de input do mouse (Update é mais estável para input)
        lookX = Input.GetAxis("Mouse X") * sensX * Time.deltaTime;
        lookY = Input.GetAxis("Mouse Y") * sensY * Time.deltaTime;
    }

    void LateUpdate()
    {
        if (!player.health.isAlive)
        {
            leftArmIK.weight = 0;
            rightArmIK.weight = 0;
            return;
        }

        if (!player.canMove)
        {
            // Enquanto não pode se mover → segue o anchor suavemente
            transform.SetParent(cameraAnchor);

            Vector3 localOffset = new Vector3(0f, 0.13f, -0.45f);
            transform.localPosition = Vector3.Lerp(transform.localPosition,
                                                   localOffset,
                                                   Time.deltaTime * 5f);

            transform.localRotation = Quaternion.identity;

            wasLocked = true;
            return;
        }
        else if (wasLocked)
        {
            // Saiu da intro → "solta" a câmera mantendo posição/rotação atuais
            if (transform.parent != null)
                transform.SetParent(null);
            wasLocked = false;
        }

        if (player.isPickingUp)
        {
            // Se estiver pegando objeto, não atualiza rotação/posição
            return;
        }
        

        // IK dos braços
        if (player.aimAnimActive)
        {
            leftArmIK.weight = Mathf.Lerp(leftArmIK.weight, 1f, Time.deltaTime * 5f);
            rightArmIK.weight = Mathf.Lerp(rightArmIK.weight, 0.7f, Time.deltaTime * 5f);
        }
        else
        {
            leftArmIK.weight = Mathf.Lerp(leftArmIK.weight, 0f, Time.deltaTime * 5f);
            rightArmIK.weight = Mathf.Lerp(rightArmIK.weight, 0f, Time.deltaTime * 5f);
        }

        // Offset dependendo do modo furtivo
        Vector3 offset = player.isStealth
            ? new Vector3(0f, 0.2f, -0.12f)
            : new Vector3(0f, 0.01f, -0.11f);

        if (transform.parent != null)
        {
            transform.SetParent(null);
        }

        // Rotação horizontal do corpo
        playerBody.Rotate(Vector3.up * lookX);

        // Rotação vertical da câmera
        xRotation = Mathf.Clamp(xRotation - lookY, -90f, 90f);

        // Posição suavizada em relação ao anchor
        Vector3 desiredPos = cameraAnchor.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position,
                                                desiredPos,
                                                ref camVelocity,
                                                followSmoothTime);

        // Aplicar rotação final
        transform.rotation = Quaternion.Euler(xRotation, playerBody.eulerAngles.y, 0f);
    }
}