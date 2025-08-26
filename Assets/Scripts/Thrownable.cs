using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Thrownable : MonoBehaviour
{
    public PlayerMoviment playerMoviment;

    [Header("Arremesso")]
    public GameObject objetoArremessavelPrefab;
    public Transform spawnPoint; // Ponto de onde o objeto será lançado
    public float forcaMin = 5f;
    public float forcaMax = 20f;
    public float velocidadeCarregamento = 10f;

    [Header("Mira (trajetória)")]
    public int pontos = 30; // número de pontos no LineRenderer
    public float tempoEntrePontos = 0.1f;

    private float forcaAtual;
    public bool carregando;
    public LineRenderer lineRenderer;

    void Start()
    {
        playerMoviment = GetComponentInParent<PlayerMoviment>();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = pontos;
    }

    void Update()
    {
        if (playerMoviment.inventory.myHandItem != null && playerMoviment.inventory.myHandItem.GetComponent<ItemArremessavel>() != null)
        {
            // Quando começar a segurar botão direito
            if (Input.GetMouseButtonDown(1))
            {
                carregando = true;
                forcaAtual = forcaMin;
            }

            // Enquanto segurar botão direito
            if (carregando && Input.GetMouseButton(1))
            {
                forcaAtual += velocidadeCarregamento * Time.deltaTime;
                forcaAtual = Mathf.Clamp(forcaAtual, forcaMin, forcaMax);

                MostrarTrajetoria();
                Debug.Log("Força atual: " + forcaAtual); // debug
            }

            // Quando soltar botão direito
            if (Input.GetMouseButtonUp(1))
            {
                carregando = false;
                lineRenderer.enabled = false;
                Arremessar();
            }
        }
    }

    void Arremessar()
    {
        if (objetoArremessavelPrefab != null && spawnPoint != null)
        {
            GameObject obj = Instantiate(objetoArremessavelPrefab, spawnPoint.position, Quaternion.identity);
            Rigidbody rb = obj.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.isKinematic = false;
                Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                rb.AddForce(ray.direction * forcaAtual, ForceMode.Impulse);
            }
        }
    }

    void MostrarTrajetoria()
    {
        lineRenderer.enabled = true;

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 posInicial = spawnPoint.position;
        Vector3 velocidadeInicial = ray.direction * forcaAtual;

        for (int i = 0; i < pontos; i++)
        {
            float tempo = i * tempoEntrePontos;
            Vector3 posicao = posInicial + velocidadeInicial * tempo;
            posicao.y += 0.5f * (Physics.gravity.y * 0.7f) * tempo * tempo;
            lineRenderer.SetPosition(i, posicao);
        }
    }
}
