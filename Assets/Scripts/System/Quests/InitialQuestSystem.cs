using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InitialQuestSystem : MonoBehaviour
{
    public static InitialQuestSystem Instance { get; set; }

    public List<levers> leversList = new List<levers>();
    public List<CircleTrain> CircleTrainList = new List<CircleTrain>();
    public List<PilarButtonSimon> pilarButtonSimons = new List<PilarButtonSimon>();
    public int CurrentLeverCount = 0;
    public bool InitialQuestStarted = false;

    public GameObject timerObjectUI;
    public bool isTimerActive = false;
    private float timerDuration = 2f;
    public float timerRemaining;
    public Slider timerBar;

    void Awake()
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

    void Start()
    {
        //Adicionar todos os levers do mapa nessa lista.
        leversList.AddRange(FindObjectsByType<levers>(FindObjectsSortMode.None));
        CircleTrainList.AddRange(FindObjectsByType<CircleTrain>(FindObjectsSortMode.None));
        pilarButtonSimons.AddRange(FindObjectsByType<PilarButtonSimon>(FindObjectsSortMode.None));

        var ui = UIreferences.Instance;
        timerObjectUI = ui.TimingBar;

        timerBar = timerObjectUI.transform.GetChild(0).GetComponent<Slider>();
        timerObjectUI.SetActive(false);
    }

    void Update()
    {
        if (CurrentLeverCount > 0f && !isTimerActive)
        {
            StartCoroutine(StartLeverTimer());
            isTimerActive = true;
        }

        if (CurrentLeverCount == leversList.Count && !InitialQuestStarted)
        {
            StartInitialQuest();
            InitialQuestStarted = true;
        }
    }

    public void StartInitialQuest()
    {
        StopAllCoroutines();
        timerObjectUI.SetActive(false);
        foreach (CircleTrain item in CircleTrainList)
        {
            item.isMoving = true;
        }

        foreach (PilarButtonSimon item in pilarButtonSimons)
        {
            item.simonPilarReset = false;
        }

        ToastMessage.Instance.ShowToast("Mecânicas desbloqueadas!!", ToastType.Success);
        Debug.LogWarning("Initial Quest Started");
    }

    private IEnumerator StartLeverTimer()
    {
        timerObjectUI.SetActive(true);
        timerRemaining = timerDuration;

        while (timerRemaining > 0f)
        {
            timerRemaining -= Time.deltaTime;
            float fill = (timerRemaining / timerDuration) * timerBar.maxValue;

            timerBar.value = fill;

            yield return null;
        }

        // Quando o tempo chega a 0.
        CurrentLeverCount = 0;
        timerObjectUI.SetActive(false);
        isTimerActive = false;

        foreach (levers lever in leversList)
        {
            lever.anim.SetTrigger("ResetLever");
            lever.leverObject.layer = LayerMask.NameToLayer("Pickable");

            yield return new WaitForSeconds(1f);
            lever.ResetTriggers();
        }
    }
}
