using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : PersistentMBPunCallbacksSingleton<NetworkManager>
{
    public enum FailTypes
    {
        CreateRoomFail,
        JoinRoomFail
    }

    [SerializeField] GameObject playerPrefab = null;

    bool joiningRoom = false;
    bool creatingNewRoom = false;
    bool loadingScene = false;

    string gameVersion;
    string handledRoomName;

    const int MinPlayersPerRoom = 2;
    const int MaxPlayersPerRoom = 2;

    static public string PlayerPrefsNameKey { private set; get; } = "PlayerName";

    static public event Action OnNamePlayerPrefNotSet;
    static public event Action OnRoomJoined;
    static public event Action<FailTypes, string> OnFail;
    static public event Action<bool> OnMatchBegun;
    static public event Action<Player> OnPlayerSpawned;

    public override void Awake()
    {
        base.Awake();

        gameVersion = Application.version;

        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnEnable()
    {
        base.OnEnable();

        SceneManager.sceneLoaded += OnLevelLoaded;

        Networking_PlayerNameInput.OnPlayerNameSaved += SetNickName;

        Networking_RoomNameInput.OnJoiningRoom += JoinRoom;
        Networking_RoomNameInput.OnCreatingNewRoom += CreateNewRoom;

        Networking_Lobby.OnMatchCountdownFinished += BeginMatch;
    }

    void Start()
    {
        if (!PlayerPrefs.HasKey(PlayerPrefsNameKey))
        {
            Debug.Log("Player name not set");
            OnNamePlayerPrefNotSet?.Invoke();
        }
        else
        {
            Debug.Log("Player name already set");
            SetNickName(PlayerPrefs.GetString(PlayerPrefsNameKey));
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();

        SceneManager.sceneLoaded -= OnLevelLoaded;

        Networking_PlayerNameInput.OnPlayerNameSaved -= SetNickName;

        Networking_RoomNameInput.OnJoiningRoom -= JoinRoom;
        Networking_RoomNameInput.OnCreatingNewRoom -= CreateNewRoom;

        Networking_Lobby.OnMatchCountdownFinished -= BeginMatch;
    }

    void OnLevelLoaded(Scene scene, LoadSceneMode mode) => loadingScene = false;

    #region Main Menu
    void SetNickName(string nickName) => PhotonNetwork.NickName = nickName;

    void ConnectToPhoton()
    {
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();
    }

    void JoinRoom(string roomName)
    {
        Debug.Log("Joining room...");

        handledRoomName = roomName;
        joiningRoom = true;

        if (PhotonNetwork.IsConnectedAndReady) PhotonNetwork.JoinRoom(roomName);
        else ConnectToPhoton();
    }

    void CreateNewRoom(string roomName)
    {
        Debug.Log("Creating new room...");

        handledRoomName = roomName;
        creatingNewRoom = true;

        if (PhotonNetwork.IsConnectedAndReady) PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = MaxPlayersPerRoom });
        else ConnectToPhoton();
    }

    void BeginMatch()
    {
        bool playingAsPA = (string)PhotonNetwork.CurrentRoom.CustomProperties["Participant A"] == PhotonNetwork.LocalPlayer.NickName;
        OnMatchBegun?.Invoke(playingAsPA);
        
        if (PhotonNetwork.IsMasterClient && !loadingScene)
        {
            loadingScene = true;
            PhotonNetwork.LoadLevel(GameManager.Get().GameplayScene);
        }
    }

    public void DisconnectFromRoom()
    {
        Debug.Log("Leaving room...");

        PhotonNetwork.LeaveRoom();
    }
    #endregion

    #region Gameplay
    public Player SpawnPlayer(Vector3 position, Quaternion rotation)
    {
        GameObject playerGO = PhotonNetwork.Instantiate(playerPrefab.name, position, rotation);
        return playerGO.GetComponent<Player>();
    }
    #endregion

    #region Overrides
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");

        if (joiningRoom) PhotonNetwork.JoinRoom(handledRoomName);
        else if (creatingNewRoom) PhotonNetwork.CreateRoom(handledRoomName, new RoomOptions { MaxPlayers = MaxPlayersPerRoom });
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Client successfully created a new room");

        creatingNewRoom = false;
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Client successfully joined a room");

        joiningRoom = false;

        if (PhotonNetwork.CurrentRoom.PlayerCount < MinPlayersPerRoom) Debug.Log("Client is waiting for an opponent");
        else Debug.Log("Matching is ready to begin");

        OnRoomJoined?.Invoke();
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount >= MinPlayersPerRoom)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount >= MaxPlayersPerRoom) PhotonNetwork.CurrentRoom.IsOpen = false;
            Debug.Log("Match is ready to begin");
        }
    }

    public override void OnDisconnected(DisconnectCause cause) => Debug.LogWarning($"Disconnected due to: { cause }");

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Create room failed (code { returnCode }): { message }");

        OnFail?.Invoke(FailTypes.CreateRoomFail, message);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Join room failed (code { returnCode }): { message }");

        OnFail?.Invoke(FailTypes.JoinRoomFail, message);
    }
    #endregion
}