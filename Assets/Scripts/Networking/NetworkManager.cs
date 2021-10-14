using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{
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

    void Awake()
    {
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

        if (PhotonNetwork.IsConnected) PhotonNetwork.JoinRoom(roomName);
        else ConnectToPhoton();
    }

    void CreateNewRoom(string roomName)
    {
        Debug.Log("Creating new room...");

        handledRoomName = roomName;
        creatingNewRoom = true;

        if (PhotonNetwork.IsConnected) PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = MaxPlayersPerRoom });
        else ConnectToPhoton();
    }

    void BeginMatch()
    {
        if (PhotonNetwork.IsMasterClient && !loadingScene)
        {
            loadingScene = true;
            PhotonNetwork.LoadLevel("Gameplay");
        }
    }

    void OnLevelLoaded(Scene scene, LoadSceneMode mode) => loadingScene = false;

    #region Overrides
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");

        if (joiningRoom) PhotonNetwork.JoinRoom(handledRoomName);
        else if (creatingNewRoom) PhotonNetwork.CreateRoom(handledRoomName, new RoomOptions { MaxPlayers = MaxPlayersPerRoom });
    }

    public override void OnDisconnected(DisconnectCause cause) => Debug.LogWarning($"Disconnected due to: { cause }");

    public override void OnJoinRoomFailed(short returnCode, string message) => Debug.LogError($"Join room failed (code { returnCode }): { message }");

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No clients are waiting for an opponent, creating a new room");

        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = MaxPlayersPerRoom });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Client successfully joined a room");

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
    #endregion
}