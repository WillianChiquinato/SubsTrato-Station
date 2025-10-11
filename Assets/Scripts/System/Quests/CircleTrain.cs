using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleTrain : MonoBehaviour
{
    [Header("Círculos (ordem do 1 ao 6)")]
    public List<Transform> circles;
    public GameObject[] DoorInitialGame;

    [Header("Intervalo de movimento (segundos)")]
    private float moveInterval = 2.6f;
    public float timingCrescing = 0f;
    public bool isMoving = false;
    private bool hasStarted = false;

    private List<Vector3> targetPositions;
    public bool OpenDooerIndex = false;
    public int doorIndex = 0;

    void Start()
    {
        targetPositions = new List<Vector3>();
        foreach (Transform circle in circles)
        {
            targetPositions.Add(circle.position);
        }
    }

    void Update()
    {
        if (isMoving)
        {
            if (!hasStarted)
            {
                StartCoroutine(MoveCirclesLoop());
                hasStarted = true;
            }

            timingCrescing += Time.deltaTime;
            if (timingCrescing >= 3f && moveInterval > 0.6f)
            {
                moveInterval -= 0.2f;
                timingCrescing = 0f;
            }
        }

        foreach (var item in circles)
        {
            if (item.GetComponent<PushDoor>() != null)
            {
                item.GetComponent<PushDoor>().door = DoorInitialGame;
            }
        }
    }

    IEnumerator MoveCirclesLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(moveInterval);

            Vector3 lastPos = targetPositions[targetPositions.Count - 1];
            for (int i = targetPositions.Count - 1; i > 0; i--)
            {
                targetPositions[i] = targetPositions[i - 1];
            }
            targetPositions[0] = lastPos;

            for (int i = 0; i < circles.Count; i++)
            {
                StartCoroutine(MoveToPosition(circles[i], targetPositions[i], 0.27f));
            }
        }
    }

    IEnumerator MoveToPosition(Transform obj, Vector3 target, float duration)
    {
        Vector3 start = obj.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            obj.position = Vector3.Lerp(start, target, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.position = target;
    }
}
