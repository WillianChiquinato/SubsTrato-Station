using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimonGame : MonoBehaviour
{
    [Header("Círculos em ordem")]
    public List<CircleButton> circles;
    public float showDelay = 0.7f;
    public int totalTurns = 8;

    private List<int> sequence = new List<int>();
    private int currentIndex = 0;
    private int currentTurn = 0;
    private bool playerTurn = false;
    private bool isShowingSequence = false;

    void Start()
    {
        StartCoroutine(StartRound());
    }

    IEnumerator StartRound()
    {
        playerTurn = false;
        isShowingSequence = true;

        if (currentTurn >= totalTurns)
        {
            Debug.Log("🏆 Você completou todas as 8 rodadas! Vitória!");
            yield break;
        }

        yield return new WaitForSeconds(1f);

        // Adiciona novo item à sequência
        sequence.Add(Random.Range(0, circles.Count));
        currentTurn++;
        Debug.Log($"▶️ Turno {currentTurn}/{totalTurns}");

        // Mostra a sequência
        for (int i = 0; i < sequence.Count; i++)
        {
            circles[sequence[i]].Highlight();
            yield return new WaitForSeconds(showDelay);
            circles[sequence[i]].Unhighlight();
            yield return new WaitForSeconds(0.2f);
        }

        isShowingSequence = false;
        playerTurn = true;
        currentIndex = 0;
    }

    public void OnCirclePressed(CircleButton circle)
    {
        if (!playerTurn || isShowingSequence) return;

        int pressedIndex = circles.IndexOf(circle);

        // Pisca o círculo pressionado (feedback visual)
        StartCoroutine(FlashPressed(circle));

        // Verifica se acertou
        if (pressedIndex == sequence[currentIndex])
        {
            currentIndex++;

            // ✅ Se acertou toda a sequência, passa de turno
            if (currentIndex >= sequence.Count)
            {
                playerTurn = false;
                StartCoroutine(StartRound());
            }
        }
        else
        {
            // ❌ ERRO — reinicia o jogo
            Debug.Log($"Errou na posição {currentIndex + 1}! Reiniciando...");
            playerTurn = false;
            isShowingSequence = false;
            sequence.Clear();
            currentTurn = 0;
            StartCoroutine(RestartAfterDelay());
        }
    }

    IEnumerator FlashPressed(CircleButton circle)
    {
        circle.Highlight();
        yield return new WaitForSeconds(0.2f);
        circle.Unhighlight();
    }

    IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        StartCoroutine(StartRound());
    }
}
