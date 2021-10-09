using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    //[SerializeField] GameObject findOpponentPanel = null;
    //[SerializeField] GameObject waitingStatusPanel = null;
    //[SerializeField] TextMeshProUGUI waitingStatusText = null;

    bool isConnecting = false;
    bool isJoiningRoom = false;
    bool isCreatingNewRoom = false;

    string gameVersion;
    string handledRoomName;

    const int MinPlayersPerRoom = 2;
    const int MaxPlayersPerRoom = 2;

    static public event Action OnEnterLobby;

    void Awake()
    {
        gameVersion = Application.version;

        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnEnable()
    {
        base.OnEnable();

        Networking_RoomManager.OnJoiningRoom += JoinRoom;
        Networking_RoomManager.OnCreatingNewRoom += CreateNewRoom;
    }

    public override void OnDisable()
    {
        base.OnDisable();

        Networking_RoomManager.OnJoiningRoom -= JoinRoom;
        Networking_RoomManager.OnCreatingNewRoom -= CreateNewRoom;
    }

    void Connect()
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
        else Connect();
    }

    void CreateNewRoom(string roomName)
    {
        Debug.Log("Creating new room...");

        handledRoomName = roomName;
        isCreatingNewRoom = true;

        if (PhotonNetwork.IsConnected) PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = MaxPlayersPerRoom });
        else Connect();
    }

    public void FindOpponent()
    {
        isConnecting = true;

        //findOpponentPanel.SetActive(false);
        //waitingStatusPanel.SetActive(true);
        //
        //waitingStatusText.text = "Searching...";

        if (PhotonNetwork.IsConnected) PhotonNetwork.JoinRandomRoom();
        else
        {
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");

        //if (isConnecting) PhotonNetwork.JoinRandomRoom();

        if (isJoiningRoom) PhotonNetwork.JoinRoom(handledRoomName);
        else if (isCreatingNewRoom) PhotonNetwork.CreateRoom(handledRoomName, new RoomOptions { MaxPlayers = MaxPlayersPerRoom });
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        //waitingStatusPanel.SetActive(false);
        //findOpponentPanel.SetActive(true);

        Debug.Log($"Disconnected due to: { cause }");
    }

    public override void OnJoinRoomFailed(short returnCode, string message) => Debug.LogError($"Join room failed (code { returnCode }): { message }");

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No clients are waiting for an opponent, creating a new room");

        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = MaxPlayersPerRoom });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Client successfully joined a room");

        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;

        if (playerCount < MinPlayersPerRoom)
        {
            //waitingStatusText.text = "Waiting for opponent";
            Debug.Log("Client is waiting for an opponent");
        }
        else
        {
            //waitingStatusText.text = "Opponent found";
            Debug.Log("Matching is ready to begin");
        }

        OnEnterLobby?.Invoke();
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
}