using UnityEngine;

public class ItemStage : MonoBehaviour
{
    public PlayerMoviment player;
    public GameObject filhoMovi;

    [Header("Flutuante item")]
    public float floatAmplitude = 0.25f;
    public float floatFrequency = 1f;
    private Vector3 startPos;

    void Start()
    {
        player = FindFirstObjectByType<PlayerMoviment>();
        startPos = transform.position;
    }

    void Update()
    {
        // Movimento sinusoidal para cima e baixo
        if (player.myHandItem != null)
        {
            // transform.position = player.myHandItem.transform.position;
            Debug.Log("Animação estatica!");
        }
        else
        {
            this.transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
            filhoMovi.transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        }
    }
}
