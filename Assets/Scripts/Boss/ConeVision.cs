using System.Collections;
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
    public AiTarget aiTarget;

    private bool playerDetectado = false;
    private Coroutine tempoVerificacao;
    private float tempoPerseguindoAposPerda = 4f;


    void Awake()
    {
        player = FindFirstObjectByType<PlayerMoviment>();
        combinedMask = visionObjectsLayer | playerLayer;
        aiTarget = GetComponentInParent<AiTarget>();
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

        bool detectouPlayerNaVisao = false;

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
                    aiTarget.target = hit.collider.transform;
                }
            }
            else
            {
                Vertices[i + 1] = VertForward * visionDistance;
            }


            Currentangle += angleIcrement;
        }

        if (detectouPlayerNaVisao)
        {
            if (!playerDetectado)
            {
                playerDetectado = true;

                // Se estava verificando, cancela
                if (tempoVerificacao != null)
                {
                    StopCoroutine(tempoVerificacao);
                }
            }
        }
        else
        {
            if (playerDetectado)
            {
                // Começa a contagem de 4 segundos se perdeu a visão
                tempoVerificacao = StartCoroutine(VerificarSePerdeuPlayer());
            }
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

    private IEnumerator VerificarSePerdeuPlayer()
    {
        playerDetectado = false;
        float tempo = 0f;

        while (tempo < tempoPerseguindoAposPerda)
        {
            yield return null;
            tempo += Time.deltaTime;

            // Se detectar o player de novo, cancela
            if (playerDetectado)
            {
                yield break;
            }
        }

        // Após 4 segundos sem detectar
        aiTarget.target = null;
        Debug.Log("Player perdido. Voltando a patrulhar.");
    }
}
