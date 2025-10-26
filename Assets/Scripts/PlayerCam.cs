using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerCam : MonoBehaviour
{
    [Header("Sensibilidade do mouse")]
    public float sensX = 100f;
    public float sensY = 100f;

    [Header("Referências")]
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
    private bool isLocalCamActive = false;

    private PlayerMoviment localPlayer;
    private Camera cam;
    private AudioListener audioListener;

    void Start()
    {
        cam = GetComponent<Camera>();
        audioListener = GetComponent<AudioListener>();

        // Garante que apenas a câmera local ficará ativa
        cam.enabled = false;
        audioListener.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        // Se ainda não encontrou o player local, procura
        if (localPlayer == null)
        {
            var players = FindObjectsOfType<PlayerMoviment>();
            foreach (var p in players)
            {
                if (p.Object.HasInputAuthority)
                {
                    localPlayer = p;
                    player = localPlayer;

                    // Ativa a câmera apenas no player local
                    cam.enabled = true;
                    audioListener.enabled = true;

                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;

                    rightArmIK.weight = 0;
                    leftArmIK.weight = 0;
                    isLocalCamActive = true;
                    break;
                }
            }
            return; // Espera até achar o player local
        }

        // Se não é a câmera local, não faz nada
        if (!isLocalCamActive)
            return;

        // Input do mouse
        lookX = Input.GetAxis("Mouse X") * sensX * Time.deltaTime;
        lookY = Input.GetAxis("Mouse Y") * sensY * Time.deltaTime;
    }

    void LateUpdate()
    {
        if (!isLocalCamActive || localPlayer == null)
            return;

        if (!player.health.isAlive)
        {
            leftArmIK.weight = 0;
            rightArmIK.weight = 0;
            return;
        }

        if (!player.canMove)
        {
            transform.SetParent(cameraAnchor);
            Vector3 localOffset = new Vector3(0f, 0.13f, -0.45f);
            transform.localPosition = Vector3.Lerp(transform.localPosition, localOffset, Time.deltaTime * 5f);
            transform.localRotation = Quaternion.identity;

            wasLocked = true;
            return;
        }
        else if (wasLocked)
        {
            if (transform.parent != null)
                transform.SetParent(null);
            wasLocked = false;
        }

        if (player.isPickingUp) return;

        // IK dos braços
        float ikLerpSpeed = Time.deltaTime * 5f;
        if (player.aimAnimActive)
        {
            leftArmIK.weight = Mathf.Lerp(leftArmIK.weight, 1f, ikLerpSpeed);
            rightArmIK.weight = Mathf.Lerp(rightArmIK.weight, 0.7f, ikLerpSpeed);
        }
        else
        {
            leftArmIK.weight = Mathf.Lerp(leftArmIK.weight, 0f, ikLerpSpeed);
            rightArmIK.weight = Mathf.Lerp(rightArmIK.weight, 0f, ikLerpSpeed);
        }

        // Offset dependendo do modo furtivo
        Vector3 offset = player.isStealth
            ? new Vector3(0f, 0.2f, -0.12f)
            : new Vector3(0f, 0.01f, -0.11f);

        if (transform.parent != null)
            transform.SetParent(null);

        // Rotação horizontal do corpo
        player.transform.Rotate(Vector3.up * lookX);

        // Rotação vertical da câmera
        xRotation = Mathf.Clamp(xRotation - lookY, -90f, 90f);

        // Posição suavizada
        Vector3 desiredPos = cameraAnchor.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref camVelocity, followSmoothTime);

        // Aplicar rotação final
        transform.rotation = Quaternion.Euler(xRotation, player.transform.eulerAngles.y, 0f);
    }
}