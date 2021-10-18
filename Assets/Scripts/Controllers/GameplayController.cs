﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayController : MonoBehaviourSingleton<GameplayController>
{
    [HideInInspector] public bool playingAsPA;

    [SerializeField] UIManager_Gameplay uiManager = null;
    [SerializeField] DialogueManager dialogueManager = null;

    [Header("Players")]
    [SerializeField] float timeToRespawnPlayer = 0.0f;
    [SerializeField] Floor[] playerAFloor = null;
    [SerializeField] Floor[] playerBFloor = null;
    [SerializeField] float[] checkPoints = null;

    [SerializeField] float spawnZPA = -18.0f;
    [SerializeField] float spawnZPB = 18.0f;
    [SerializeField] float spawnY = 6.0f;

    Player player = null;
    int currentCheckpoint = 0;

    [SerializeField] ButtonMissingPart paButtonMP = null;
    [SerializeField] ButtonMissingPart pbButtonMP = null;
    ButtonMissingPart buttonMissingPart;

    [Header("Timer")]
    [SerializeField] float timerInitialDuration = 20.0f;
    [SerializeField] float timerMistakeDecrease = 5.0f;
    float timer;

    bool timerOn = false;

    [Header("\"Spot the differences\" puzzle")]
    [SerializeField] List<Difference> paDifferences = null;
    [SerializeField] List<Difference> pbDifferences = null;
    
    List<Interactable> differences;

    [Header("\"Create Shape\" puzzle")]

    public int shapeCorrect3dShape;
    public int shapeCorrectColor;
    public int shapeCorrectSymbol;
    [SerializeField] DeliveryMachine deliveryMachine;
    bool secondPhase = false;
    [SerializeField] Door secondPhaseDoorA;
    [SerializeField] Door secondPhaseDoorB;
    Door secondPhaseDoor;

    public static event Action<float,bool> OnTimerUpdated;
    public static event Action OnLevelEnd;

    void OnEnable()
    {
        Difference.OnSelected += CheckSelectedDifference;
        DoorButton.OnTimerTriggered += StartTimer;
        LevelEnd.OnLevelEndReached += ProcessLevelEnd;
        CodeBar.UpdatePuzzleProgress += UpdateCSProgress;
    }

    void Start()
    {
        player = NetworkManager.Get().SpawnPlayer(GetPlayerSpawnPosition(), Quaternion.identity);
        player.RespawnAtCheckpoint = RespawnPlayer;

        differences = new List<Interactable>();
        if (playingAsPA)
        {
            buttonMissingPart = paButtonMP;
            foreach (Interactable difference in paDifferences) differences.Add(difference);
            secondPhaseDoor = secondPhaseDoorA;
        }
        else
        {
            buttonMissingPart = pbButtonMP;
            foreach (Interactable difference in pbDifferences) differences.Add(difference);
            secondPhaseDoor = secondPhaseDoorB;
        }
        deliveryMachine.UpdateShapeCorrectFeatures(shapeCorrectColor, shapeCorrect3dShape, shapeCorrectSymbol);

        timer = timerInitialDuration;
        OnTimerUpdated?.Invoke(timer,false);

        dialogueManager.PlayDialogueLine(dialogueManager.categories[(int)DialogueManager.DialogueCategories.PuzzleIntructions].lines[0]);
    }

    void OnDisable()
    {
        Difference.OnSelected -= CheckSelectedDifference;
        DoorButton.OnTimerTriggered -= StartTimer;
        LevelEnd.OnLevelEndReached -= ProcessLevelEnd;
        CodeBar.UpdatePuzzleProgress -= UpdateCSProgress;
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

    void RespawnPlayer()
    {
        player.transform.position = GetPlayerSpawnPosition();
        timer = timerInitialDuration;
        OnTimerUpdated?.Invoke(timer, false);

        switch (currentCheckpoint)
        {
            case 0:
                differences.Clear();
                if (playingAsPA) foreach (Interactable difference in paDifferences) differences.Add(difference);
                else foreach (Interactable difference in pbDifferences) differences.Add(difference);
                uiManager.UpdatePuzzleInfoText(differences.Count, false);
                break;
            case 1:
                secondPhase = false;
                break;
        }
    }

    Vector3 GetPlayerSpawnPosition()
    {
        float spawnZ = playingAsPA ? spawnZPA : spawnZPB;
        return new Vector3(checkPoints[currentCheckpoint], spawnY, spawnZ);
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
        timer = timerInitialDuration;
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
    void CheckSelectedDifference(Difference selectedDifference)
    {
        if (differences.Contains(selectedDifference))
        {
            differences.Remove(selectedDifference);
            uiManager.UpdatePuzzleInfoText(differences.Count, true);
            if (differences.Count <= 0)
            {
                timerOn = false;
                buttonMissingPart.gameObject.SetActive(true);
                uiManager.PuzzleInfoTextActiveState(false);
                currentCheckpoint++;
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

    #region Create Shapes
    void UpdateCSProgress() //CS = Create Shapes puzzle
    {
        if (!secondPhase)
            secondPhaseDoor.Open();
        else
            buttonMissingPart.gameObject.SetActive(true);
    }
    
    #endregion
}