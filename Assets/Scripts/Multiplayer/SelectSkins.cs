using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectSkins : MonoBehaviour
{
    [Header("Settings")]
    public GameObject playerDetect;
    public GameObject HUDSelected;
    private CanvasGroup canvasGroup;


    [Header("Skin Selection")]
    public int skinIndex = 0;
    public GameObject[] skins;
    private string[] skinNames = { "Cobaia", "Cientista Masculino", "Cientista Feminina", "Anão Zelador" };
    public Image ImageInteract;
    public Button NextButton;
    public Button PreviousButton;
    public Button ConfirmButton;

    void Awake()
    {
        skins[skinIndex].SetActive(true);

        for (int i = 1; i < skins.Length; i++)
        {
            skins[i].SetActive(false);
        }
    }

    void Start()
    {
        NextButton.onClick.AddListener(NextSkin);
        PreviousButton.onClick.AddListener(PreviousSkin);
        ConfirmButton.onClick.AddListener(FinishedSkin);

        canvasGroup = HUDSelected.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        HUDSelected.SetActive(false);

        ImageInteract.gameObject.SetActive(false);
    }

    void Update()
    {
        //Fazer a animação rodar em 360 graus
        skins[skinIndex].transform.Rotate(Vector3.up * 40f * Time.deltaTime);
        HUDSelected.transform.Find("SkinName").GetComponent<TextMeshProUGUI>().text = skinNames[skinIndex];
    }

    public void NextSkin()
    {
        skins[skinIndex].SetActive(false);
        skinIndex = (skinIndex + 1) % skins.Length;
        skins[skinIndex].SetActive(true);
    }

    public void PreviousSkin()
    {
        skins[skinIndex].SetActive(false);
        skinIndex = (skinIndex - 1 + skins.Length) % skins.Length;
        skins[skinIndex].SetActive(true);
    }

    public void OpenHUDSelected()
    {
        if (playerDetect)
        {
            StartCoroutine(FadeCanvas(1f));
        }
    }

    public void FinishedSkin()
    {
        // Salvar a skin selecionada
        if (playerDetect)
        {
            CharacterMultiplayer character = playerDetect.GetComponent<CharacterMultiplayer>();
            if (character != null)
            {
                character.selectedSkinIndex = skinIndex;
                character.RpcUpdateSkin(skinIndex);
                character.canMove = true;
            }
        }
        StartCoroutine(FadeCanvas(0f));

        Invoke(nameof(DisableHUD), 0.8f);
    }

    void DisableHUD()
    {
        HUDSelected.SetActive(false);
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerDetect = other.gameObject;
            ImageInteract.gameObject.SetActive(true);
            if (this.playerDetect && Input.GetKeyDown(KeyCode.E))
            {
                OpenHUDSelected();
                playerDetect.GetComponent<CharacterMultiplayer>().canMove = false;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            ImageInteract.gameObject.SetActive(false);
        }
    }

    private IEnumerator FadeCanvas(float targetAlpha)
    {
        HUDSelected.SetActive(true);
        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < 0.8f)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / 0.8f);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        // Se quiser desabilitar interação quando invisível:
        canvasGroup.interactable = targetAlpha > 0.9f;
        canvasGroup.blocksRaycasts = targetAlpha > 0.9f;
    }
}
