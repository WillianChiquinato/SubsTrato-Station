using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-100)]
public class UIreferences : MonoBehaviour
{
    public static UIreferences Instance { get; set; }

    [Header("Player References")]
    public GameObject player;

    [Header("UI References")]
    public GameObject DeathUI;
    public GameObject PickUpItemUI;
    public RectTransform SlotHighlight;
    public RectTransform[] slotsPositions;
    public Image[] slotIcons;
    public GameObject AmmoSlot;
    public GameObject TimingBar;
    public GameObject OrbesCountUI;
    public GameObject textObj;

    public GameObject EstaminaBarUI;

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
