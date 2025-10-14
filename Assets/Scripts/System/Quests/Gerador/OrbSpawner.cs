using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrbSpawner : MonoBehaviour
{
    public static OrbSpawner Instance { get; set; }

    public GameObject orbPrefab;
    public GameObject ChipDropPrefab;
    public GameObject orbeObject;
    public List<Transform> spawnPoints;
    public int orbCount = 3;

    private List<GameObject> spawnedOrbs = new List<GameObject>();
    public int collectedOrbs = 0;

    public GameObject timerObjectUI;
    public bool isTimerActive = false;
    public float puzzleDuration = 10f;
    public float timerRemaining;
    public Slider timerBar;

    private bool puzzleActive = false;
    public GameObject orbesCountUI;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            var ui = UIreferences.Instance;
            timerObjectUI = ui.TimingBar;
            orbesCountUI = ui.OrbesCountUI;

            timerBar = timerObjectUI.transform.GetChild(0).GetComponent<Slider>();
            timerObjectUI.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        orbesCountUI.SetActive(puzzleActive);
        orbesCountUI.GetComponentInChildren<TextMeshProUGUI>().text = collectedOrbs + "/" + orbCount;

        if (collectedOrbs >= orbCount)
        {
            EndPuzzle();

            collectedOrbs = 0;
            timerObjectUI.SetActive(false);
            isTimerActive = false;
            puzzleActive = false;
        }
    }

    public void StartPuzzle()
    {
        ClearOrbs();
        SpawnOrbs();
        if (!puzzleActive)
        {
            StartCoroutine(StartTimerBarOrbs());
            puzzleActive = true;
        }
    }

    void SpawnOrbs()
    {
        List<int> usedIndexes = new List<int>();

        for (int i = 0; i < orbCount; i++)
        {
            int randomIndex;
            do
            {
                randomIndex = Random.Range(0, spawnPoints.Count);
            } while (usedIndexes.Contains(randomIndex));

            usedIndexes.Add(randomIndex);

            GameObject orb = Instantiate(orbPrefab, spawnPoints[randomIndex].position, Quaternion.identity);
            spawnedOrbs.Add(orb);
        }
    }

    void ClearOrbs()
    {
        foreach (var orb in spawnedOrbs)
        {
            if (orb != null)
            {
                Destroy(orb);
            }
        }
        spawnedOrbs.Clear();
    }

    private IEnumerator StartTimerBarOrbs()
    {
        timerObjectUI.SetActive(true);
        isTimerActive = true;
        timerRemaining = puzzleDuration;

        while (timerRemaining > 0f)
        {
            timerRemaining -= Time.deltaTime;
            float fill = (timerRemaining / puzzleDuration) * timerBar.maxValue;

            timerBar.value = fill;

            yield return null;
        }

        collectedOrbs = 0;
        timerObjectUI.SetActive(false);
        isTimerActive = false;

        EndPuzzleFail();
    }

    void EndPuzzleFail()
    {
        Debug.Log("Tempo acabou! Puzzle falhou ou terminou.");
        collectedOrbs = 0;
        if (orbeObject == null)
        {
            orbeObject = Instantiate(ChipDropPrefab, transform.position + new Vector3(0, 1, -3), Quaternion.identity);
        }
        puzzleActive = false;

        ToastMessage.Instance.ShowToast("Tempo esgotado! Recomece", ToastType.Error);
        StopAllCoroutines();
        ClearOrbs();
    }

    void EndPuzzle()
    {
        Debug.Log("Puzzle terminado com sucesso!");
        ToastMessage.Instance.ShowToast("Puzzle completo! Continue", ToastType.Success);
        collectedOrbs = 0;
        puzzleActive = false;

        ChipSystem.Instance.chipSystemCount++;
        StopAllCoroutines();
        ClearOrbs();
    }
}
