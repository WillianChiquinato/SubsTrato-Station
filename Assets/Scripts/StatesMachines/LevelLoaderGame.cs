using System.Collections;
using Fusion;
using UnityEngine;

public class LevelLoaderGame : MonoBehaviour
{
    public Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void Transicao(SceneRef cena, NetworkRunner runner)
    {
        StartCoroutine(LoadSceneFusion(cena, runner));
    }

    private IEnumerator LoadSceneFusion(SceneRef cena, NetworkRunner runner)
    {
        if (animator != null)
        {
            animator.SetTrigger("start");
            yield return new WaitForSeconds(1.1f);
        }

        Debug.Log("Iniciando carregamento da cena com Fusion...");
        runner.LoadScene(cena);
    }
}
