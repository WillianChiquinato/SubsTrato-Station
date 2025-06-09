using System;
using System.Collections;
using Fusion;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class LevelLoaderAsync : MonoBehaviour
{
    public Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void TransicaoComCallback(Action onFadeComplete)
    {
        StartCoroutine(PlayTransitionThenCallback(onFadeComplete));
    }

    private IEnumerator PlayTransitionThenCallback(Action callback)
    {
        if (animator != null)
        {
            animator.SetTrigger("start");
            yield return new WaitForSeconds(1.1f);
        }

        callback?.Invoke();
    }
}