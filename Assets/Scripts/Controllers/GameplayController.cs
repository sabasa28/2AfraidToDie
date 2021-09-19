using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayController : MonoBehaviourSingleton<GameplayController>
{
    [HideInInspector] public bool playingAsPA;

    [SerializeField] UIManager_Gameplay uiManager = null;
    [SerializeField] DialogueManager dialogueManager = null;

    [Header("Players")]
    [SerializeField] Player player = null;

    [SerializeField] float timeToRespawnPlayer = 0.0f;
    [SerializeField] Floor[] playerAFloor = null;
    [SerializeField] Floor[] playerBFloor = null;
    [SerializeField] float[] checkPoints = null;
    int currentCheckpoint = 0;

    [SerializeField] float spawnZPA = -18.0f;
    [SerializeField] float spawnZPB = 18.0f;
    [SerializeField] float spawnY = 6.0f;

    [SerializeField] ButtonMissingPart paButtonMP = null;
    [SerializeField] ButtonMissingPart pbButtonMP = null;
    ButtonMissingPart buttonMissingPart;

    [Header("Timer")]
    [SerializeField] float timerInitialDuration = 20.0f;
    [SerializeField] float timerMistakeDecrease = 5.0f;
    float timer;

    bool timerOn = false;

    [Header("\"Spot the differences\" puzzle")]
    [SerializeField] List<Interactable> paDifferences = null;
    [SerializeField] List<Interactable> pbDifferences = null;
    
    List<Interactable> differences;

    [Header("\"Create Shape\" puzzle")]
    public int shapeCorrect3dShape;
    public int shapeCorrectColor;
    public int shapeCorrectSymbol;
    [SerializeField] DeliveryMachine deliveryMachine;

    public static event Action<float,bool> OnTimerUpdated;
    public static event Action OnLevelEnd;

    void OnEnable()
    {
        Player.OnDifferenceObjectSelected += CheckSelectedDifferenceObject;
        DoorButton.OnTimerTriggered += StartTimer;
        LevelEnd.OnLevelEndReached += ProcessLevelEnd;
    }

    void Start()
    {
        player.RespawnAtCheckpoint = RespawnPlayer;

        LocatePlayerOnSpawnPosition();

        differences = new List<Interactable>();
        if (playingAsPA)
        {
            buttonMissingPart = paButtonMP;
            foreach (Interactable difference in paDifferences) differences.Add(difference);
        }
        else
        {
            buttonMissingPart = pbButtonMP;
            foreach (Interactable difference in pbDifferences) differences.Add(difference);
        }
        deliveryMachine.UpdateShapeCorrectFeatures(shapeCorrectColor, shapeCorrect3dShape, shapeCorrectSymbol);

        timer = timerInitialDuration;
        OnTimerUpdated?.Invoke(timer,false);

        dialogueManager.PlayDialogueLine(dialogueManager.categories[(int)DialogueManager.DialogueCategories.PuzzleIntructions].lines[0]);
    }

    void OnDisable()
    {
        Player.OnDifferenceObjectSelected -= CheckSelectedDifferenceObject;
        DoorButton.OnTimerTriggered -= StartTimer;
        LevelEnd.OnLevelEndReached -= ProcessLevelEnd;
    }

    void ProcessLevelEnd()
    {
        player.movementController.SetCursorLockState(false);
        player.movementController.SetRotationActiveState(false);
        player.movementController.setCharacterControllerActiveState(false);

        OnLevelEnd?.Invoke();
    }

    #region Player
    void OpenPlayersFloor()
    {
        Floor[] floorsToOpen = playerAFloor;
        if (!playingAsPA) floorsToOpen = playerBFloor;

        for (int i = 0; i < floorsToOpen.Length; i++)
        {
            floorsToOpen[i].Open();
        }

        player.Fall();
    }

    void LocatePlayerOnSpawnPosition()
    {
        
        float zPosToRespawn = spawnZPA;
        if (!playingAsPA) zPosToRespawn = spawnZPB;
        player.transform.position = new Vector3(checkPoints[currentCheckpoint], spawnY, zPosToRespawn);
    }

    void RespawnPlayer()
    {
        LocatePlayerOnSpawnPosition();

        timer = timerInitialDuration;
        OnTimerUpdated?.Invoke(timer, false);

        differences.Clear();
        if (playingAsPA) foreach (Interactable difference in paDifferences) differences.Add(difference);
        else foreach (Interactable difference in pbDifferences) differences.Add(difference);
        uiManager.UpdatePuzzleInfoText(differences.Count, false);
    }
    #endregion

    #region Timer
    void StartTimer()
    {
        Debug.Log("timer started");
        uiManager.puzzleVariableName = "Differences Left"; //Cuando tengamos varios puzzles esto se pondria en una variable que cambia segun el puzzle
        uiManager.PuzzleInfoTextActiveState(true); //si el puzzle no tiene algo que mostrar se dejaria apagado
        uiManager.UpdatePuzzleInfoText(differences.Count, false);
        timerOn = true;
        StartCoroutine(Timer());
    }

    IEnumerator Timer()
    {
        while (timerOn)
        {
            timer -= Time.deltaTime;
            OnTimerUpdated?.Invoke(timer, false);

            if (timer <= 0.0f)
            {
                OnTimerUpdated?.Invoke(timer, true);

                timerOn = false;
                Debug.Log("you lost");
                OpenPlayersFloor();
            }

            yield return null;
        }
    }
    #endregion

    #region Spot the Differences
    void CheckSelectedDifferenceObject(Interactable selectedObject)
    {
        if (differences.Contains(selectedObject))
        {
            differences.Remove(selectedObject);
            uiManager.UpdatePuzzleInfoText(differences.Count, true);
            if (differences.Count <= 0)
            {
                timerOn = false;
                buttonMissingPart.gameObject.SetActive(true);
                uiManager.PuzzleInfoTextActiveState(false);
                Debug.Log("you win");
            }
        }
        else
        {
            OnPlayerMistake();
        }
    }

    void OnPlayerMistake()
    {
        timer -= timerMistakeDecrease;
        OnTimerUpdated?.Invoke(timer, true);
    }
    #endregion
}