using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayController : MonoBehaviourSingleton<GameplayController>
{
    [HideInInspector] public bool playingAsPA;

    [SerializeField] UIManager_Gameplay uiManager = null;
    [SerializeField] DialogueManager dialogueManager = null;
    PhotonView photonView;

    Player player;

    [Header("Spawn")]
    [SerializeField] float timeToRespawnPlayer = 0.0f;
    [SerializeField] float spawnZPA = -18.0f;
    [SerializeField] float spawnZPB = 18.0f;
    [SerializeField] float spawnY = 6.0f;

    [Header("Room flow")]
    [SerializeField] float[] checkPoints = null;
    int currentCheckpoint = 0;

    [Space]
    [SerializeField] Door[] paDoors = null;
    [SerializeField] Door[] pbDoors = null;
    Door[] doors;
    int currentDoor = 0;

    [Space]
    [SerializeField] Transform[] paButtonMPPos = null;
    [SerializeField] Transform[] pbButtonMPPos = null;
    Transform buttonMissingPartPos;

    [Space]
    [SerializeField] Floor[] playerAFloor = null;
    [SerializeField] Floor[] playerBFloor = null;

    [Header("Timer")]
    [SerializeField] float timerInitialDuration = 20.0f;
    [SerializeField] float timerMistakeDecrease = 5.0f;

    bool timerOn = false;
    float timer;

    const string AreDoorsUnlockedProp = "AreDoorsUnlocked";

    [Header("\"Spot the differences\" puzzle")]
    [SerializeField] List<Difference> paDifferences = null;
    [SerializeField] List<Difference> pbDifferences = null;
    int differencesSelected = 0;
    bool canSelectDifference = false;

    List<Interactable> differences;

    [Header("\"Create Shape\" puzzle")]

    [SerializeField] ShapeBuilder shapeBuilder;
    public int shapeCorrect3dShape;
    public int shapeCorrectColor;
    public int shapeCorrectSymbol;
    int currentCode;
    [SerializeField] DeliveryMachine deliveryMachineA;
    [SerializeField] DeliveryMachine deliveryMachineB;
    [SerializeField] Phone phoneA;
    [SerializeField] Phone phoneB;
    bool secondPhase = false;
    [SerializeField] Door secondPhaseDoorA;
    [SerializeField] Door secondPhaseDoorB;
    Door secondPhaseDoor;

    public int DifferenceCount { get { return paDifferences.Count; } }
    public float TimerDuration { get { return timerInitialDuration; } }

    public static event Action<float,bool> OnTimerUpdated;
    public static event Action OnLevelEnd;

    public override void Awake()
    {
        base.Awake();

        photonView = GetComponent<PhotonView>();
    }

    void OnEnable()
    {
        Door.OnDoorUnlocked += UnlockDoor;
        Door.OnDoorOpen += OpenDoorOnOtherPlayers;
        Door.OnDoorClosed += CloseDoorOnOtherPlayers;
        Door.OnTimerTriggered += StartTimer;
        LevelEnd.OnLevelEndReached += ProcessLevelEnd;

        Difference.OnSelected += CheckSelectedDifference;

        CodeBar.UpdatePuzzleProgress += UpdateCSProgress;
        Phone.OnCorrectNumberInserted += UpdateCSProgress;
    }

    void Start()
    {
        player = NetworkManager.Get().SpawnPlayer(GetPlayerSpawnPosition(), Quaternion.identity);
        player = FindObjectOfType<Player>();
        player.RespawnAtCheckpoint = RespawnPlayer;

        doors = playingAsPA ? paDoors : pbDoors;

        SetUpAreDoorsUnlockedProp();

        SetUpSpotTheDifferences();
        SetUpCreateShapes();

        timer = timerInitialDuration;
        OnTimerUpdated?.Invoke(timer, false);

        dialogueManager.PlayDialogueLine(dialogueManager.categories[(int)DialogueManager.DialogueCategories.PuzzleIntructions].lines[0]);
    }

    void OnDisable()
    {
        Door.OnDoorUnlocked -= UnlockDoor;
        Door.OnDoorOpen -= OpenDoorOnOtherPlayers;
        Door.OnDoorClosed -= CloseDoorOnOtherPlayers;
        Door.OnTimerTriggered -= StartTimer;
        LevelEnd.OnLevelEndReached -= ProcessLevelEnd;

        Difference.OnSelected -= CheckSelectedDifference;

        CodeBar.UpdatePuzzleProgress -= UpdateCSProgress;
        Phone.OnCorrectNumberInserted -= UpdateCSProgress;
    }

    void ProcessLevelEnd()
    {
        player.movementController.SetCursorLockState(false);
        player.movementController.SetRotationActiveState(false);
        player.movementController.SetCharacterControllerActiveState(false);

        OnLevelEnd?.Invoke();
    }

    #region Spawn
    void RespawnPlayer()
    {
        player.transform.position = GetPlayerSpawnPosition();
        timer = timerInitialDuration;
        canSelectDifference = false;
        OnTimerUpdated?.Invoke(timer, false);

        switch (currentCheckpoint)
        {
            case 0:
                differences.Clear();
                if (playingAsPA) foreach (Interactable difference in paDifferences) differences.Add(difference);
                else foreach (Interactable difference in pbDifferences) differences.Add(difference);

                differencesSelected = 0;
                uiManager.UpdatePuzzleInfoText(differencesSelected, false);
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

    #region Room flow
    void SetUpAreDoorsUnlockedProp()
    {
        bool[] areDoorsUnlocked = new bool[PhotonNetwork.CurrentRoom.PlayerCount];
        for (int i = 0; i < areDoorsUnlocked.Length; i++) areDoorsUnlocked[i] = false;

        ExitGames.Client.Photon.Hashtable property = new ExitGames.Client.Photon.Hashtable();
        property.Add(AreDoorsUnlockedProp, areDoorsUnlocked);
        PhotonNetwork.CurrentRoom.SetCustomProperties(property);
    }

    void UnlockDoor()
    {
        int participantIndex = (int)PhotonNetwork.LocalPlayer.CustomProperties[NetworkManager.PlayerPropParticipantIndex];
        bool[] areDoorsUnlocked = (bool[])PhotonNetwork.CurrentRoom.CustomProperties[AreDoorsUnlockedProp];
        areDoorsUnlocked[participantIndex] = true;

        foreach (bool item in areDoorsUnlocked)
        {
            if (!item)
            {
                ExitGames.Client.Photon.Hashtable property = new ExitGames.Client.Photon.Hashtable();
                property.Add(AreDoorsUnlockedProp, areDoorsUnlocked);
                PhotonNetwork.CurrentRoom.SetCustomProperties(property);

                return;
            }
        }

        photonView.RPC("OpenCurrentDoor", RpcTarget.All);
    }

    void OpenDoorOnOtherPlayers(bool playerA, int doorNumber) => photonView.RPC("OnPlayerDoorOpen", RpcTarget.Others, playerA, doorNumber);

    void CloseDoorOnOtherPlayers(bool playerA, int doorNumber) => photonView.RPC("OnPlayerDoorClosed", RpcTarget.Others, playerA, doorNumber);

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
    #endregion

    #region Timer
    void StartTimer()
    {
        uiManager.puzzleVariableName = "Differences Left"; //Cuando tengamos varios puzzles esto se pondria en una variable que cambia segun el puzzle
        canSelectDifference = true;

        differencesSelected = 0;
        uiManager.UpdatePuzzleInfoText(differencesSelected, false);

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
                OpenPlayersFloor();
            }

            yield return null;
        }
    }
    #endregion

    #region Puzzles
    void WinPuzzle()
    {
        timerOn = false;
        canSelectDifference = false;

        DroneController.Get().SendDrone(buttonMissingPartPos.position);
        uiManager.PuzzleInfoTextActiveState(false);
        currentCheckpoint++;

        if (playingAsPA) buttonMissingPartPos = paButtonMPPos[currentCheckpoint];
        else buttonMissingPartPos = pbButtonMPPos[currentCheckpoint];
    }
    #endregion

    #region Spot the Differences
    void SetUpSpotTheDifferences()
    {
        differences = new List<Interactable>();
        if (playingAsPA)
        {
            buttonMissingPartPos = paButtonMPPos[currentCheckpoint];
            foreach (Interactable difference in paDifferences) differences.Add(difference);
            secondPhaseDoor = secondPhaseDoorA;
        }
        else
        {
            buttonMissingPartPos = pbButtonMPPos[currentCheckpoint];
            foreach (Interactable difference in pbDifferences) differences.Add(difference);
            secondPhaseDoor = secondPhaseDoorB;
        }
    }

    void CheckSelectedDifference(Difference selectedDifference)
    {
        if (!canSelectDifference) return;

        if (differences.Contains(selectedDifference))
        {
            differences.Remove(selectedDifference);

            differencesSelected++;
            uiManager.UpdatePuzzleInfoText(differencesSelected, true);

            if (differences.Count <= 0) WinPuzzle();
        }
        else OnPlayerMistake();
    }

    void OnPlayerMistake()
    {
        timer -= timerMistakeDecrease;

        OnTimerUpdated?.Invoke(timer, true);
    }
    #endregion

    #region Create Shapes
    void SetUpCreateShapes()
    {
        shapeBuilder.GetRandomShape(out shapeCorrect3dShape, out shapeCorrectColor, out shapeCorrectSymbol);
        if (!secondPhase)
        {
            deliveryMachineB.SetShapePuzzleVars(shapeCorrect3dShape, shapeCorrectColor, shapeCorrectSymbol, out currentCode);
            phoneA.correctNumber = currentCode;
        }
        else
        {
            deliveryMachineA.SetShapePuzzleVars(shapeCorrect3dShape, shapeCorrectColor, shapeCorrectSymbol, out currentCode);
            phoneB.correctNumber = currentCode;
        }
    }

    void UpdateCSProgress() //CS = Create Shapes puzzle
    {
        if (!secondPhase)
        {
            secondPhase = true;
            secondPhaseDoor.Open();
            SetUpCreateShapes();
        }
        else
        {
            DroneController.Get().SendDrone(buttonMissingPartPos.position);
        }
    }
    #endregion

    #region RPCs
    [PunRPC]
    void OpenCurrentDoor()
    {
        doors[currentDoor].Open();
        currentDoor++;
        SetUpAreDoorsUnlockedProp();
    }

    [PunRPC]
    void OnPlayerDoorOpen(bool playerA, int doorNumber)
    {
        Door[] playerDoors = playerA ? paDoors : pbDoors;
        playerDoors[doorNumber].Open(false);
    }

    [PunRPC]
    void OnPlayerDoorClosed(bool playerA, int doorNumber)
    {
        Door[] playerDoors = playerA ? paDoors : pbDoors;
        playerDoors[doorNumber].Close(false);
    }
    #endregion
}