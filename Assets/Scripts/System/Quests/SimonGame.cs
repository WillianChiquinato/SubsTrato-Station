using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimonGame : MonoBehaviour
{
    [Header("Círculos em ordem")]
    public List<CircleButton> circles;
    public CircleTrain[] circleTrain;
    public float showDelay = 0.7f;
    public int totalTurns = 8;
    public GameObject PilarBtn;

    private List<int> Sequence = new List<int>();
    private int currentIndex = 0;
    private int currentTurn = 0;
    private bool playerTurn = false;
    private bool isShowingSequence = false;
    private bool canClick = true;

    public int victoryIndex = 0;

    void Start()
    {
        for (int i = 0; i < circles.Count; i++)
        {
            Debug.Log($"Círculo {i}: {circles[i].name}");
        }
    }

    public IEnumerator StartRound()
    {
        playerTurn = false;
        isShowingSequence = true;
        ToastMessage.Instance.ShowToast("Observe a sequência!", ToastType.Alert);

        if (currentTurn >= totalTurns)
        {
            Debug.Log("🏆 Você completou todas as 8 rodadas! Vitória!");
            ToastMessage.Instance.RemoveAllToast();
            StartCoroutine(ShowVictoryMessage());
            yield break;
        }

        yield return new WaitForSeconds(1.5f);

        // Adiciona novo item à sequência
        Sequence.Add(Random.Range(0, circles.Count));
        currentTurn++;
        Debug.Log($"▶️ Turno {currentTurn}/{totalTurns}");

        // Mostra a sequência
        for (int i = 0; i < Sequence.Count; i++)
        {
            circles[Sequence[i]].Highlight();
            yield return new WaitForSeconds(showDelay);
            circles[Sequence[i]].Unhighlight();
            yield return new WaitForSeconds(0.2f);
        }

        isShowingSequence = false;
        playerTurn = true;
        currentIndex = 0;
    }

    public void OnCirclePressed(CircleButton circle)
    {
        if (!playerTurn || isShowingSequence) return;
        if (!canClick) return;

        canClick = false;
        StartCoroutine(EnableClickDelay());

        int pressedIndex = circles.IndexOf(circle);

        // Pisca o círculo pressionado (feedback visual)
        StartCoroutine(FlashPressed(circle));

        // Verifica se acertou
        if (pressedIndex == Sequence[currentIndex])
        {
            currentIndex++;

            // ✅ Se acertou toda a sequência, passa de turno
            if (currentIndex >= Sequence.Count)
            {
                playerTurn = false;
                StartCoroutine(StartRound());
                ToastMessage.Instance.ShowToast("Acertou a sequência, continua!", ToastType.Success);
            }
        }
        else
        {
            // ❌ ERRO — reinicia o jogo
            Debug.Log($"Errou na posição {currentIndex + 1}! Reiniciando...");
            playerTurn = false;
            isShowingSequence = false;
            Sequence.Clear();
            currentTurn = 0;
            PilarBtn.GetComponent<PilarButtonSimon>().simonPilarReset = false;
            ToastMessage.Instance.ShowToast("Errou a sequência, resetando...", ToastType.Error);
        }
    }

    private IEnumerator EnableClickDelay()
    {
        yield return new WaitForSeconds(0.25f);
        canClick = true;
    }

    IEnumerator FlashPressed(CircleButton circle)
    {
        circle.Highlight();
        yield return new WaitForSeconds(0.2f);
        circle.Unhighlight();
    }

    IEnumerator ShowVictoryMessage()
    {
        yield return new WaitForSeconds(1.5f);

        ToastMessage.Instance.ShowToast("Você completou todas as rodadas! Vitória!", ToastType.Success);
        PilarBtn.GetComponent<PilarButtonSimon>().simonPilarReset = true;
        Sequence.Clear();
        currentTurn = 0;
        playerTurn = false;
        isShowingSequence = false;

        victoryIndex = Random.Range(0, circles.Count);
        circles[victoryIndex].Highlight();
        var hightLight = circles[victoryIndex].gameObject.GetComponent<HightLights>();

        if (hightLight != null)
        {
            Destroy(hightLight);
        }

        foreach (var item in circleTrain)
        {
            item.doorIndex = victoryIndex;

            var circleIndex = circles.IndexOf(circles[victoryIndex]);
            item.circles[circleIndex].gameObject.AddComponent<PushDoor>();
            item.circles[circleIndex].gameObject.AddComponent<MeshCollider>();
        }
    }
}