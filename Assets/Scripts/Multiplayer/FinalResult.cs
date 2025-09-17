using System.Collections;
using UnityEngine;

public class FinalResult : MonoBehaviour
{
    public GameObject HUDSelected;
    private CanvasGroup canvasGroup;

    public LevelTransicao levelLoader;

    void Awake()
    {
        levelLoader = FindFirstObjectByType<LevelTransicao>();
        canvasGroup = HUDSelected.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        HUDSelected.SetActive(false);
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            StartCoroutine(FadeCanvas(1f));
        }
    }

    private IEnumerator FadeCanvas(float targetAlpha)
    {
        HUDSelected.SetActive(true);
        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < 1f)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / 1f);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        // Se quiser desabilitar interação quando invisível:
        canvasGroup.interactable = targetAlpha > 0.9f;
        canvasGroup.blocksRaycasts = targetAlpha > 0.9f;

        yield return new WaitForSeconds(1f);
        levelLoader.Transicao("TelaMenu");
    }
}
