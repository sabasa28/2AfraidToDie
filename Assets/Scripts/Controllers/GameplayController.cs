using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayController : MonoBehaviour
{
    [HideInInspector] public bool playingAsPA;

    [SerializeField] UIManager_Gameplay uiManager = null;

    [Header("Players")]
    [SerializeField] Player player = null;

    [SerializeField] float TimeToRespawnPlayer = 0.0f;
    [SerializeField] Floor[] playerAFloor = null;
    [SerializeField] Floor[] playerBFloor = null;
    [SerializeField] float[] checkPoints = null;
    int currentCheckpoint = 0;

    [SerializeField] float RespawnZPA = -18.0f;
    [SerializeField] float RespawnZPB = 18.0f;

    [SerializeField] Door paDoor = null;
    [SerializeField] Door pbDoor = null;
    Door door;

    [Header("Timer")]
    [SerializeField] float timerInitialDuration = 20.0f;
    [SerializeField] float timerMistakeDecrease = 5.0f;
    float timer;

    bool timerOn = false;

    [Header("\"Spot the differences\" puzzle")]
    [SerializeField] List<Interactable> paDifferences = null;
    [SerializeField] List<Interactable> pbDifferences = null;

    List<Interactable> differences;

    static public event Action<float> OnTimerUpdated;

    void OnEnable()
    {
        DoorButton.OnDoorOpen += StartTimer;

        Player.OnDifferenceObjectSelected += CheckSelectedDifferenceObject;
    }

    void Start()
    {
        player.RespawnAtCheckpoint = RespawnPlayer;

        differences = new List<Interactable>();
        if (playingAsPA)
        {
            door = paDoor;
            foreach (Interactable difference in paDifferences) differences.Add(difference);
        }
        else
        {
            door = pbDoor;
            foreach (Interactable difference in pbDifferences) differences.Add(difference);
        }

        timer = timerInitialDuration;
        OnTimerUpdated?.Invoke(timer);
    }

    void OnDisable()
    {
        DoorButton.OnDoorOpen -= StartTimer;

        Player.OnDifferenceObjectSelected -= CheckSelectedDifferenceObject;
    }

    #region Player Death
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
        float zPosToRespawn = RespawnZPA;
        if (!playingAsPA) zPosToRespawn = RespawnZPB;
        player.transform.position = new Vector3(checkPoints[currentCheckpoint], 0.0f, zPosToRespawn);

        timer = timerInitialDuration;
        OnTimerUpdated?.Invoke(timer);

        differences.Clear();
        if (playingAsPA) foreach (Interactable difference in paDifferences) differences.Add(difference);
        else foreach (Interactable difference in pbDifferences) differences.Add(difference);
    }
    #endregion

    #region Timer
    void StartTimer()
    {
        timerOn = true;
        StartCoroutine(Timer());
    }

    IEnumerator Timer()
    {
        while (timerOn)
        {
            timer -= Time.deltaTime;
            OnTimerUpdated?.Invoke(timer);

            if (timer <= 0.0f)
            {
                OnTimerUpdated?.Invoke(timer);

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
            Debug.Log("difference selected. remaining differences: " + differences.Count);

            if (differences.Count <= 0)
            {
                timerOn = false;
                door.Open();
                Debug.Log("you win");
            }
        }
        else
        {
            timer -= timerMistakeDecrease;
            OnTimerUpdated?.Invoke(timer);
        }
    }
    #endregion
}