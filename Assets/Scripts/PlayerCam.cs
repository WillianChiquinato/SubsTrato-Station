using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX = 100f;
    public float sensY = 100f;

    public Transform playerBody;
    public PlayerMoviment player;
    Vector3 cameraRotation;

    void Start()
    {
        player = playerBody.GetComponent<PlayerMoviment>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cameraRotation = transform.localEulerAngles;
    }


    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensX * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensY * Time.deltaTime;

        if (player.health.isAlive)
        {
            // Atualiza o player (só Y)
            playerBody.Rotate(Vector3.up * mouseX);

            // Atualiza rotação local da câmera (X e Z)
            cameraRotation.x -= mouseY;
            cameraRotation.x = Mathf.Clamp(cameraRotation.x, -90f, 90f);

            // Se não for filha, você precisa montar a rotação global manualmente:
            Quaternion playerRotation = Quaternion.Euler(0f, playerBody.eulerAngles.y, 0f);
            Quaternion cameraLocalRotation = Quaternion.Euler(cameraRotation);

            transform.rotation = playerRotation * cameraLocalRotation;
        }
        else
        {
            // Se o jogador não estiver vivo, reseta a rotação da câmera
            cameraRotation.x = 0f;
            cameraRotation.y = 0f;
            cameraRotation.z = 0f;
            transform.localEulerAngles = cameraRotation;
            playerBody.rotation = Quaternion.Euler(0f, playerBody.eulerAngles.y, 0f);
        }
    }
}
