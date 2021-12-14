using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameplayController : MonoBehaviourSingleton<GameplayController>
{
    [HideInInspector] public bool playingAsPA;

    [SerializeField] UIManager_Gameplay uiManager = null;
    NetworkManager networkManager;
    PhotonView photonView;

    [SerializeField] GameObject playerPrefab;
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
    ButtonMissingPart currentButtonMP;

    [Space]
    [SerializeField] Floor[] playersFloor = null;

    [Header("Timer")]
    [SerializeField] float timerInitialDuration = 20.0f;
    [SerializeField] float timerMistakeDecrease = 5.0f;

    bool timerOn = false;
    float timer;

    const string AreDoorsUnlockedProp = "AreDoorsUnlocked";

    [Header("PUZZLES")]
    [SerializeField] FindTheDifferences[] puzzlesPrefabs;
    List<FindTheDifferences> puzzles;
    FindTheDifferences currentPuzzle;
    [SerializeField] float spaceBetweenPuzzles;
    [SerializeField] Transform puzzlesParent;
    
    [Header("\"Spot the differences\" puzzle")]
    [SerializeField] List<Difference> paDifferences = null;
    [SerializeField] List<Difference> pbDifferences = null;
    [SerializeField] int differencesAmount = 0;
    int differencesSelected = 0;
    bool canSelectDifference = false;
    bool initialSetDone = false;

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

    public bool OnPause { private set; get; }
    public int DifferenceCount { get { return paDifferences.Count; } }
    public float TimerDuration { get { return timerInitialDuration; } }

    public static event Action<float> OnTimerUpdated;
    public static event Action OnPlayerMistake;
    public static event Action OnPlayerGuess;
    public static event Action OnAllDifferencesSelected;
    public static event Action OnPuzzleWon;
    public static event Action OnLevelEnd;

    public override void Awake()
    {
        base.Awake();

        networkManager = NetworkManager.Get();
        photonView = GetComponent<PhotonView>();
    }

    void OnEnable()
    {
        UIManager_Gameplay.OnPauseToggled += TogglePause;
        Player.OnRespawn += RespawnPlayer;

        Door.OnDoorUnlocked += UnlockDoor;
        Door.OnEntranceDoorOpen += StartTimer;
        LevelEnd.OnLevelEndReached += ProcessLevelEnd;

        Difference.OnSelected += CheckSelectedDifference;

        CodeBar.UpdatePuzzleProgress += UpdateCSProgress;
        Phone.OnCorrectNumberInserted += UpdateCSProgress;
    }

    void Start()
    {
        InstantiatePuzzles();
        //player = NetworkManager.Get().SpawnPlayer(GetPlayerSpawnPosition(), Quaternion.identity);
        player = Instantiate(playerPrefab, GetPlayerSpawnPosition(), Quaternion.identity).GetComponent<Player>();

        SetUpDoors();

        SetUpSpotTheDifferences(puzzles[0]);
        SetUpCreateShapes();

        timer = timerInitialDuration;
        OnTimerUpdated?.Invoke(timer);
    }

    void OnDisable()
    {
        UIManager_Gameplay.OnPauseToggled -= TogglePause;
        Player.OnRespawn -= RespawnPlayer;

        Door.OnDoorUnlocked -= UnlockDoor;
        Door.OnEntranceDoorOpen -= StartTimer;
        LevelEnd.OnLevelEndReached -= ProcessLevelEnd;

        Difference.OnSelected -= CheckSelectedDifference;

        CodeBar.UpdatePuzzleProgress -= UpdateCSProgress;
        Phone.OnCorrectNumberInserted -= UpdateCSProgress;
    }

    void TogglePause(bool onPause) => OnPause = onPause;

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
        player.movementController.characterController.enabled = false;
        player.transform.position = GetPlayerSpawnPosition();
        player.movementController.characterController.enabled = true;

        timer = timerInitialDuration;
        canSelectDifference = false;
        OnTimerUpdated?.Invoke(timer);

        switch (currentCheckpoint)
        {
            case 0:
                SetUpSpotTheDifferences(currentPuzzle);
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
    void SetUpDoors()
    {
        doors = playingAsPA ? paDoors : pbDoors;
        foreach (Door door in doors)
        {
            for (int i = 0; i < networkManager.PlayerCount; i++)
                if (networkManager.GetPlayerByIndex(i, out Photon.Realtime.Player player)) door.PlayerScreens[i].PlayerName = player.NickName;
        }

        SetUpAreDoorsUnlockedProp();
    }

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
        networkManager.GetPlayerIndex(PhotonNetwork.LocalPlayer, out int playerIndex);
        photonView.RPC("SetPlayerScreenOn", RpcTarget.All, playerIndex, true);

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

    void Lose()
    {
        currentDoor--;
        if (currentDoor < 0) currentDoor = 0;

        for (int i = 0; i < playersFloor.Length; i++)
        {
            playersFloor[i].Open();
        }

        player.Fall();
    }
    #endregion

    #region Timer
    void StartTimer()
    {
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
            OnTimerUpdated?.Invoke(timer);

            if (timer <= 0.0f)
            {
                timerOn = false;
                Lose();
            }

            yield return null;
        }
    }
    #endregion

    #region Puzzles
    void InstantiatePuzzles()
    {
        puzzles = new List<FindTheDifferences>();
        for (int i = 0; i < puzzlesPrefabs.Length; i++)
        {
            puzzles.Add(Instantiate(puzzlesPrefabs[i], new Vector3(spaceBetweenPuzzles * i, 0.0f, 0.0f), Quaternion.identity, puzzlesParent));
        }
    }

    void SendPulserToPlayer()
    {
        currentButtonMP = DroneController.Get().SendDrone(buttonMissingPartPos.position);
    }

    void WinPuzzle()
    {
        timerOn = false;
        canSelectDifference = false;
        player.movementController.rigidBody.useGravity = false;
        player.movementController.rigidBody.constraints = RigidbodyConstraints.FreezePositionY;

        currentCheckpoint++;

        SetUpSpotTheDifferences(puzzles[currentCheckpoint]);

        if (playingAsPA) buttonMissingPartPos = paButtonMPPos[currentCheckpoint];
        else buttonMissingPartPos = pbButtonMPPos[currentCheckpoint];

        OnPuzzleWon?.Invoke();
    }
    #endregion

    #region Spot the Differences
    void SetUpSpotTheDifferences(FindTheDifferences findTheDifferences)
    {
        currentPuzzle = findTheDifferences;
        currentPuzzle.SetSide(playingAsPA);

        if (PhotonNetwork.IsMasterClient)
        {
            SetRandomDifferences();
        }
    }

    void SetRandomDifferences()
    {
        int numOfInteractables = currentPuzzle.GetInteractablesNumber();
        int[] differencesIndices = new int[differencesAmount];
        bool[] baseStateOfObjects = new bool[numOfInteractables];
        for (int i = 0; i < differencesAmount; i++)
        {
            int a = UnityEngine.Random.Range(0, numOfInteractables);
            if (differencesIndices.Contains(a)) i--;
            else differencesIndices[i] = a;
        }
        for (int i = 0; i < baseStateOfObjects.Length; i++)
        {
            baseStateOfObjects[i] = UnityEngine.Random.Range(0, 2) == 1? true : false;
            Debug.Log(baseStateOfObjects[i]);
        }
        photonView.RPC("SetDifferences", RpcTarget.All, differencesIndices, baseStateOfObjects);
    }

    void SetInteractablesAsDifferences(int[] differencesIndices, bool[] baseStateOfObjs)
    {
        currentPuzzle.ClearDifferences();
        differencesSelected = 0;
        uiManager.UpdatePuzzleInfoText(differencesSelected, true);
        
        currentPuzzle.SetDifferences(differencesIndices, baseStateOfObjs);
        
        if (playingAsPA)
        {
            buttonMissingPartPos = paButtonMPPos[currentCheckpoint];
            secondPhaseDoor = secondPhaseDoorA; //esto se saca despues, es del segundo puzzle
        }
        else
        {
            buttonMissingPartPos = pbButtonMPPos[currentCheckpoint];
            secondPhaseDoor = secondPhaseDoorB; //esto se saca despues, es del segundo puzzle
        }
    }

    void CheckSelectedDifference(Difference selectedDifference)
    {
        if (!canSelectDifference) return;

        if (currentPuzzle.CheckInteractable(selectedDifference))
        {
            differencesSelected++;
            uiManager.UpdatePuzzleInfoText(differencesSelected, true);

            OnPlayerGuess?.Invoke();

            if (currentPuzzle.GetDifferencesLeft() <= 0)
            {
                SendPulserToPlayer();
                OnAllDifferencesSelected?.Invoke();
            }
        }
        else OnMistake();
    }

    void OnMistake()
    {
        photonView.RPC("DecreaseTimer", RpcTarget.All);

        OnPlayerMistake?.Invoke();
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
    void SetDifferences(int[] differences, bool[] baseStateOfObjs)
    {
        SetInteractablesAsDifferences(differences, baseStateOfObjs);
    }

    [PunRPC]
    void SetPlayerScreenOn(int playerIndex, bool on) => doors[currentDoor].PlayerScreens[playerIndex].On = on;

    [PunRPC]
    void OpenCurrentDoor()
    {
        doors[currentDoor].Open();
        currentDoor++;
        SetUpAreDoorsUnlockedProp();
        if (differencesSelected == differencesAmount) WinPuzzle();
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

    [PunRPC]
    void DecreaseTimer() => timer -= timerMistakeDecrease;
    #endregion
}