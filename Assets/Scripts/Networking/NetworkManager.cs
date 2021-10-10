using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public struct RoomData
    {
        public string Name { set; get; }

        public int PlayerCount { set; get; }
        public List<string> PlayerNames { set; get; }
    }

    bool isJoiningRoom = false;
    bool isCreatingNewRoom = false;

    string gameVersion;
    string handledRoomName;

    const int MinPlayersPerRoom = 2;
    const int MaxPlayersPerRoom = 2;

    static public RoomData CurrentRoom { private set; get; }
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

        Networking_PlayerNameInput.OnPlayerNameSaved += SetNickName;

        Networking_RoomNameInput.OnJoiningRoom += JoinRoom;
        Networking_RoomNameInput.OnCreatingNewRoom += CreateNewRoom;
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

        Networking_PlayerNameInput.OnPlayerNameSaved -= SetNickName;

        Networking_RoomNameInput.OnJoiningRoom -= JoinRoom;
        Networking_RoomNameInput.OnCreatingNewRoom -= CreateNewRoom;
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
        isJoiningRoom = true;

        if (PhotonNetwork.IsConnected) PhotonNetwork.JoinRoom(roomName);
        else ConnectToPhoton();
    }

    void CreateNewRoom(string roomName)
    {
        Debug.Log("Creating new room...");

        handledRoomName = roomName;
        isCreatingNewRoom = true;

        if (PhotonNetwork.IsConnected) PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = MaxPlayersPerRoom });
        else ConnectToPhoton();
    }

    void SetUpCurrentRoomData()
    {
        List<string> playerNames = new List<string>();
        for (int i = 1; i <= PhotonNetwork.CurrentRoom.PlayerCount; i++)
        {
            PhotonNetwork.CurrentRoom.Players.TryGetValue(i, out Photon.Realtime.Player player);
            if (player != null)
            {
                Debug.Log(player.NickName);
                playerNames.Add(player.NickName);
            }
        }

        CurrentRoom = new RoomData
        {
            Name = PhotonNetwork.CurrentRoom.Name,
            PlayerCount = PhotonNetwork.CurrentRoom.PlayerCount,
            PlayerNames = playerNames
        };
    }

    #region Overrides
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");

        if (isJoiningRoom) PhotonNetwork.JoinRoom(handledRoomName);
        else if (isCreatingNewRoom) PhotonNetwork.CreateRoom(handledRoomName, new RoomOptions { MaxPlayers = MaxPlayersPerRoom });
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

        SetUpCurrentRoomData();

        if (CurrentRoom.PlayerCount < MinPlayersPerRoom) Debug.Log("Client is waiting for an opponent");
        else Debug.Log("Matching is ready to begin");

        OnRoomJoined?.Invoke();
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount >= MinPlayersPerRoom)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount >= MaxPlayersPerRoom) PhotonNetwork.CurrentRoom.IsOpen = false;

            //waitingStatusText.text = "Opponent found";
            Debug.Log("Match is ready to begin");

            PhotonNetwork.LoadLevel("Gameplay");
        }
    }
    #endregion
}