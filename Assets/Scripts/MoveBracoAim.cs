using UnityEngine;

public class MoveBracoAim : MonoBehaviour
{
    public Transform cameraTransform;
    public Transform neck;
    public Transform spine2;

    void LateUpdate()
    {
        float verticalRotation = cameraTransform.localEulerAngles.x;
        if (verticalRotation > 180f) verticalRotation -= 360f;

        // Aplica suavemente a rotação na coluna e no pescoço
        Vector3 neckRotation = neck.localEulerAngles;
        neckRotation.x = verticalRotation * 0.4f;
        neck.localEulerAngles = neckRotation;

        Vector3 spineRotation = spine2.localEulerAngles;
        spineRotation.x = verticalRotation * 0.3f;
        spine2.localEulerAngles = spineRotation;
    }
}
