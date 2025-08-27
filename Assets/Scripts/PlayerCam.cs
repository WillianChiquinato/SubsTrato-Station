using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerCam : MonoBehaviour
{
    public float sensX = 100f;
    public float sensY = 100f;

    public Transform playerBody;
    public PlayerMoviment player;
    private float xRotation = 0f;
    private float mouseX;
    private float mouseY;

    public TwoBoneIKConstraint rightArmIK;
    public TwoBoneIKConstraint leftArmIK;

    public Transform cameraAnchor;
    public float followSpeed = 15f;
    private bool wasLocked = true;

    void Start()
    {
        player = playerBody.GetComponent<PlayerMoviment>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        rightArmIK.weight = 0;
        leftArmIK.weight = 0;
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
            // Enquanto não pode se mover → só segue o anchor sem resetar posição bruscamente
            transform.SetParent(cameraAnchor);

            Vector3 localOffset = new Vector3(0f, 0.13f, -0.45f);
            transform.localPosition = Vector3.Lerp(transform.localPosition, localOffset, Time.deltaTime * 5f);

            transform.localRotation = Quaternion.identity;

            wasLocked = true;
            return;
        }
        else
        {
            if (wasLocked)
            {
                // Saiu da intro → "solta" a câmera mantendo posição/rotação atuais
                transform.SetParent(null);
                wasLocked = false;
            }
        }

        if (player.isPickingUp)
        {
            // Talvez você queira ajustar os alvos do IK ou pesos durante o "picking up"
            return;
        }

        if (player.aimAnimActive)
        {
            leftArmIK.weight = Mathf.Lerp(leftArmIK.weight, 1, Time.deltaTime * 5f);
            rightArmIK.weight = Mathf.Lerp(rightArmIK.weight, 0.7f, Time.deltaTime * 5f);
        }
        else
        {
            leftArmIK.weight = Mathf.Lerp(leftArmIK.weight, 0, Time.deltaTime * 5f);
            rightArmIK.weight = Mathf.Lerp(rightArmIK.weight, 0, Time.deltaTime * 5f);
        }

        Vector3 offset = Vector3.zero;


        if (player.isStealth)
        {
            offset = new Vector3(0f, 0.2f, -0.12f);
        }
        else
        {
            offset = new Vector3(0f, 0.01f, -0.11f);
        }

        transform.SetParent(null);

        float mouseX = Input.GetAxis("Mouse X") * sensX * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensY * Time.deltaTime;

        // rotação do corpo (horizontal)
        playerBody.Rotate(Vector3.up * mouseX);

        // rotação vertical só do mouse
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // posição suavizada (só pega a posição do anchor)
        Vector3 desiredPos = cameraAnchor.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * followSpeed);

        // rotação da câmera só pelo mouse, ignorando balanço da cabeça
        transform.rotation = Quaternion.Euler(xRotation, playerBody.eulerAngles.y, 0f);
    }
}