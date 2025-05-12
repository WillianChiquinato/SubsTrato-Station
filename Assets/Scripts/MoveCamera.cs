using UnityEngine;

public class MovePlayer : MonoBehaviour
{
    public Transform cameraPos;

    void Update()
    {
        transform.position = cameraPos.position;
    }
}
