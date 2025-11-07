using UnityEngine;
using UnityEngine.Events;

public class PilarButtonSimon : MonoBehaviour, IUsable
{
    [field: SerializeField] public UnityEvent OnUse { get; private set; }
    public GameObject SimonGameObject;
    public bool simonPilarReset = false;

    public AudioSource audioSourceBtnInitial;

    void Start()
    {
        simonPilarReset = true;
    }

    public void Use(GameObject actor)
    {
        OnUse?.Invoke();
    }

    void Update()
    {
        if (simonPilarReset)
        {
            gameObject.layer = LayerMask.NameToLayer("Default");
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("Pickable");
        }
    }

    public void OnClickSimonGame()
    {
        if (SimonGameObject != null)
        {
            var simonGame = SimonGameObject.GetComponent<SimonGame>();
            if (simonGame != null)
            {
                simonGame.StartCoroutine(simonGame.StartRound());
                
                if (audioSourceBtnInitial != null && !audioSourceBtnInitial.isPlaying)
                {
                    try
                    {
                        AudioSource.PlayClipAtPoint(audioSourceBtnInitial.clip, transform.position);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"Erro ao tocar som: {e.Message}");
                    }
                }
                
                simonPilarReset = true;
                Debug.Log("🎮 Simon Game iniciado!");
            }
        }
    }
}
