using TMPro;
using UnityEngine;

public class ChipSystem : MonoBehaviour
{
    public static ChipSystem Instance { get; set; }

    public int chipSystemCount = 0;
    public bool AberturaFinal = false;
    public GameObject DoorFinal;

    public OrbSpawner orbSpawner;
    public GameObject countChipsUI;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        orbSpawner = OrbSpawner.Instance;

        var ui = UIreferences.Instance;
        countChipsUI = ui.textObj;
    }

    void Update()
    {
        countChipsUI.GetComponent<TextMeshProUGUI>().text = chipSystemCount + "/3";

        if (chipSystemCount >= 3)
        {
            AberturaFinal = true;
            if (AberturaFinal)
            {
                DoorFinal.GetComponent<Animator>().SetTrigger("Open");
                DoorFinal.GetComponent<BoxCollider>().enabled = false;
            }
        }
    }

    public void Interact()
    {
        Debug.Log("Interacted with chip: " + gameObject.name);
        chipSystemCount++;
    }
}
