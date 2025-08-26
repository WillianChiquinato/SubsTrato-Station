using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;

public class QuestSystem : MonoBehaviour
{
    public static QuestSystem instance;

    public QuestTrigger currentQuestTrigger;
    public TextMeshProUGUI questTextArea;
    public QuestTextos linhaAtual;
    public Queue<QuestTextos> linhas;
    public TMP_InputField inputField;
    public Button btnSubmit;
    public float speedTexto = 0.2f;
    public bool isTextComplete = false;

    public bool questArea = false;
    public bool isQuestAtivo = false;
    public bool questEnding = false;

    Animator animator;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        linhas = new Queue<QuestTextos>();
    }

    public void StartQuest(Quest quest, QuestTrigger trigger)
    {
        questArea = true;
        isQuestAtivo = true;
        animator.SetBool("QuestStart", true);

        linhas.Clear();

        foreach (QuestTextos questTextos in quest.dialogoTextos)
        {
            linhas.Enqueue(questTextos);
        }

        currentQuestTrigger = trigger;
        DisplayNextLinha();
    }

    public void DisplayNextLinha()
    {
        isTextComplete = false;

        linhaAtual = linhas.Dequeue();

        StopAllCoroutines();
        StartCoroutine(Sequencial(linhaAtual));
    }

    IEnumerator Sequencial(QuestTextos questTextos)
    {
        questTextArea.text = "";
        foreach (char letter in questTextos.linhaTexto.ToCharArray())
        {
            questTextArea.text += letter;
            yield return new WaitForSeconds(speedTexto);
        }
        isTextComplete = true;
    }

    public void EndQuest()
    {
        questEnding = true;
        isQuestAtivo = false;
        animator.SetBool("QuestStart", false);

        isTextComplete = true;
    }

    public void ButtonSubmitQuest()
    {
        if (isTextComplete)
        {
            SubmitResult();
        }
        else
        {
            StopAllCoroutines();
            questTextArea.text = linhaAtual.linhaTexto;
            isTextComplete = true;
        }
    }

    public void SubmitResult()
    {
        Debug.Log("Input jogador " + inputField.text);
        if (inputField != null && !string.IsNullOrEmpty(inputField.text))
        {
            if (inputField.text.Equals(linhaAtual.respostaQuest))
            {
                Debug.Log("Quest concluída com sucesso!");
                ToastMessage.Instance.ShowToast("Quest concluída com sucesso!", ToastType.Success);
                if (linhas.Count == 0)
                {
                    Debug.Log("Quest FINALIZADA!!");
                    EndQuest();
                    if (currentQuestTrigger != null)
                    {
                        currentQuestTrigger.EndQuest();
                    }
                    return;
                }
                questArea = false;
            }
            else if (inputField.text.Equals(linhaAtual.Similar))
            {
                ToastMessage.Instance.ShowToast("Esta muito perto!!", ToastType.Alert);
            }
            else
            {
                Debug.Log("Resposta incorreta. Tente novamente.");
                ToastMessage.Instance.ShowToast("Resposta incorreta. Tente novamente.", ToastType.Error);
            }
        }
        else
        {
            Debug.Log("Por favor, insira uma resposta.");
            ToastMessage.Instance.ShowToast("Por favor, insira uma resposta.", ToastType.Alert);
        }
    }
}
