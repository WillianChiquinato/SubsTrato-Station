using ExitGames.Client.Photon.StructWrapping;
using UnityEngine;
using UnityEngine.Events;

public class levers : MonoBehaviour
{
    [field: SerializeField] public UnityEvent OnUse { get; private set; }
    public GameObject leverObject;

    [Header("Sounds")]
    public AudioSource SoundLever;

    [Header("Settings")]
    public Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        leverObject = transform.GetChild(1).transform.GetChild(2).gameObject;
    }

    public void Use()
    {
        OnUse?.Invoke();
    }

    public void ResetTriggers()
    {
        anim.ResetTrigger("NewQuest");
        anim.ResetTrigger("ResetLever");
    }

    public void UseLeversInCorrectTiming()
    {
        anim.SetTrigger("NewQuest");

        // if (SoundLever != null)
        // {
        //     AudioSource.PlayClipAtPoint(SoundLever.clip, transform.position);
        // }
        if (leverObject != null)
        {
            leverObject.layer = LayerMask.NameToLayer("Default");   
        }
        InitialQuestSystem.Instance.CurrentLeverCount++;
    }
}
