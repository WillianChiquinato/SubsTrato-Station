using UnityEngine;

[DefaultExecutionOrder(-100)]
public class UIreferencesLobby : MonoBehaviour
{
    public static UIreferencesLobby Instance { get; set; }

    [Header("Player References")]
    public GameObject player;

    [Header("UI References")]
    public GameObject MostrarBtn;
    public GameObject ViewObjs;
    public GameObject SelectedSkinsUI;

    private void Awake()
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
}
