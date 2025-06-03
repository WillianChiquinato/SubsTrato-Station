using UnityEngine;

public class MoveBracoAim : MonoBehaviour
{
    public Transform cameraTransform;
    public Transform neck;
    public Transform spine1;

    private float maxNeckAngle = 20f;
    private float maxSpineAngle = 45f;

    void LateUpdate()
    {
        float verticalRotation = cameraTransform.localEulerAngles.x;
        if (verticalRotation > 180f) verticalRotation -= 360f;

        // Aplica suavemente a rotação na coluna e no pescoço
        float neckX = Mathf.Clamp(verticalRotation * 0.3f, -maxNeckAngle, maxNeckAngle);
        float spineX = Mathf.Clamp(verticalRotation * 0.5f, -maxSpineAngle, maxSpineAngle);

        Vector3 neckRotation = neck.localEulerAngles;
        neckRotation.x = neckX;
        neck.localEulerAngles = neckRotation;

        Vector3 spineRotation = spine1.localEulerAngles;
        spineRotation.x = spineX;
        spine1.localEulerAngles = spineRotation;
    }
}
