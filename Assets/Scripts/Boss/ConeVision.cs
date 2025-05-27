using Unity.VisualScripting;
using UnityEngine;

public class ConeVision : MonoBehaviour
{
    public PlayerMoviment player;
    public Material visionMaterial;
    public float visionAngle = 45f;
    public float visionDistance = 10f;

    public LayerMask visionObjectsLayer;
    public LayerMask playerLayer;
    LayerMask combinedMask;

    public int visionConeResolution = 120;
    public Mesh VisionMesh;
    MeshFilter meshFilter;

    void Awake()
    {
        player = FindFirstObjectByType<PlayerMoviment>();
        combinedMask = visionObjectsLayer | playerLayer;
    }

    void Start()
    {
        transform.AddComponent<MeshRenderer>().material = visionMaterial;
        meshFilter = transform.AddComponent<MeshFilter>();
        VisionMesh = new Mesh();
        visionAngle *= Mathf.Deg2Rad;
    }

    void Update()
    {
        DrawVisionCone();
    }

    void DrawVisionCone()
    {
        int[] triangles = new int[(visionConeResolution - 1) * 3];
        Vector3[] Vertices = new Vector3[visionConeResolution + 1];
        Vertices[0] = Vector3.zero;
        float Currentangle = -visionAngle / 2;
        float angleIcrement = visionAngle / (visionConeResolution - 1);
        float Sine;
        float Cosine;

        for (int i = 0; i < visionConeResolution; i++)
        {
            Sine = Mathf.Sin(Currentangle);
            Cosine = Mathf.Cos(Currentangle);
            Vector3 RaycastDirection = (transform.forward * Cosine) + (transform.right * Sine);
            Vector3 VertForward = (Vector3.forward * Cosine) + (Vector3.right * Sine);
            if (Physics.Raycast(transform.position, RaycastDirection, out RaycastHit hit, visionDistance, combinedMask))
            {
                Vertices[i + 1] = VertForward * hit.distance;
                if (((1 << hit.collider.gameObject.layer) & playerLayer) != 0)
                {
                    Debug.Log("Player detectado!");
                    // Você pode chamar aqui alguma função de perseguição, ataque, etc.
                }
            }
            else
            {
                Vertices[i + 1] = VertForward * visionDistance;
            }


            Currentangle += angleIcrement;
        }
        for (int i = 0, j = 0; i < triangles.Length; i += 3, j++)
        {
            triangles[i] = 0;
            triangles[i + 1] = j + 1;
            triangles[i + 2] = j + 2;
        }
        VisionMesh.Clear();
        VisionMesh.vertices = Vertices;
        VisionMesh.triangles = triangles;
        meshFilter.mesh = VisionMesh;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player is in vision cone");
        }
    }
}
