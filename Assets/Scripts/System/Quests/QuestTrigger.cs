using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestTextos
{
    public string tituloQuest;
    [TextArea(3, 20)]
    public string linhaTexto;
    public string respostaQuest = "";
    public string Similar = "";
}

[System.Serializable]
public class Quest
{
    public List<QuestTextos> dialogoTextos = new List<QuestTextos>();
}

public class QuestTrigger : MonoBehaviour
{
    [Header("Texto da Quest")]
    public Quest quest;
    public Animator animator;

    public void TriggerQuest()
    {
        QuestSystem.instance.StartQuest(quest, this);
        Debug.Log("Quest iniciada!");
    }

    public void EndQuest()
    {
        animator.SetBool("OpenTrigger", true);
        Debug.Log("Quest finalizada!");
    }
}
