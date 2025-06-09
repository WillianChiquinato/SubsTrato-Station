using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerCam : MonoBehaviour
{
    public float sensX = 100f;
    public float sensY = 100f;

    public Transform playerBody;
    public PlayerMoviment player;
    public Transform target; // Este parece ser o alvo da câmera (cabeça)
    private float xRotation = 0f;
    private float mouseX;
    private float mouseY;

    public TwoBoneIKConstraint rightArmIK;
    public TwoBoneIKConstraint leftArmIK;

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

        if (player.isPickingUp)
        {
            xRotation = 0f;
            // Talvez você queira ajustar os alvos do IK ou pesos durante o "picking up"
            return;
        }

        // Entrada do mouse
        mouseX = Input.GetAxis("Mouse X") * sensX * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * sensY * Time.deltaTime;

        // Rotação horizontal no corpo
        playerBody.Rotate(Vector3.up * mouseX);

        // Rotação vertical no alvo (cabeça)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        if (target)
        {
            target.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            // Agora a câmera segue posição e rotação do alvo
            Vector3 offset = new Vector3(0f, 0.2f, -0.1f);
            transform.position = target.position + target.rotation * offset;
            transform.rotation = target.rotation;
        }
    }
}