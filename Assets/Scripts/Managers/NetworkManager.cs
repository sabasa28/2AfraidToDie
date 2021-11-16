using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
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

    Room currentRoom;
    RoomOptions defaultRoomOptions;

    public Dictionary<string, Photon.Realtime.Player> PlayersByID { private set; get; } = new Dictionary<string, Photon.Realtime.Player>();
    
    const int MaxPlayersPerRoom = 2;

    static public string PlayerPropParticipantIndex { set; get; } = "ParticipantIndex";
    static public string PlayerPrefsNameKey { private set; get; } = "PlayerName";

    static public event Action<bool> OnNamePlayerPrefNotSet;
    static public event Action<string> OnPlayerNameSet;
    static public event Action OnRoomJoined;
    static public event Action<FailTypes, string> OnFail;
    static public event Action<bool> OnMatchBegun;
    static public event Action<Player> OnPlayerSpawned;

    public override void Awake()
    {
        base.Awake();

        gameVersion = Application.version;
        defaultRoomOptions = new RoomOptions{ PublishUserId = true, MaxPlayers = MaxPlayersPerRoom };

        PhotonNetwork.AutomaticallySyncScene = true;
        SetPlayerPropParticipantIndex(-1);
    }

    public override void OnEnable()
    {
        base.OnEnable();

        SceneManager.sceneLoaded += OnLevelLoaded;

        TitleScreen.OnTitleScreenClosed += VerifyPlayerName;

        Networking_PlayerNameInput.OnPlayerNameSaved += SetPlayerName;
        Networking_RoomNameInput.OnJoiningRoom += JoinRoom;
        Networking_RoomNameInput.OnCreatingNewRoom += CreateNewRoom;
        Networking_Lobby.OnMatchCountdownFinished += BeginMatch;
    }

    void Start() => PhotonNetwork.AuthValues = new AuthenticationValues(Guid.NewGuid().ToString());

    public override void OnDisable()
    {
        base.OnDisable();

        SceneManager.sceneLoaded -= OnLevelLoaded;

        TitleScreen.OnTitleScreenClosed -= VerifyPlayerName;

        Networking_PlayerNameInput.OnPlayerNameSaved -= SetPlayerName;
        Networking_RoomNameInput.OnJoiningRoom -= JoinRoom;
        Networking_RoomNameInput.OnCreatingNewRoom -= CreateNewRoom;
        Networking_Lobby.OnMatchCountdownFinished -= BeginMatch;
    }

    void OnLevelLoaded(Scene scene, LoadSceneMode mode) => loadingScene = false;

    void VerifyPlayerName()
    {
        if (!PlayerPrefs.HasKey(PlayerPrefsNameKey))
        {
            Debug.Log("Player name not set");
            OnNamePlayerPrefNotSet?.Invoke(true);
        }
        else
        {
            Debug.Log("Player name already set");

            PhotonNetwork.NickName = PlayerPrefs.GetString(PlayerPrefsNameKey);
            OnPlayerNameSet?.Invoke(PhotonNetwork.NickName);
        }
    }

    public bool GetPlayerByIndex(int playerIndex, out Photon.Realtime.Player player)
    {
        int foundPlayers = 0;
        int actorIndex = 1;
    
        while (foundPlayers < currentRoom.PlayerCount)
        {
            if (currentRoom.Players.TryGetValue(actorIndex, out Photon.Realtime.Player foundPlayer))
            {
                if (foundPlayers == playerIndex)
                {
                    player = foundPlayer;
                    return true;
                }
    
                foundPlayers++;
            }
    
            actorIndex++;
        }

        player = null;
        return false;
    }

    public bool GetPlayerByParticipantIndex(int participantIndex, out Photon.Realtime.Player player)
    {
        foreach (Photon.Realtime.Player playerInRoom in currentRoom.Players.Values)
        {
            if ((int)playerInRoom.CustomProperties[PlayerPropParticipantIndex] == participantIndex)
            {
                player = playerInRoom;
                return true;
            }
        }

        player = null;
        return false;
    }

    public bool GetPlayerIndex(Photon.Realtime.Player player, out int index)
    {
        int foundPlayers = 0;
        int actorIndex = 1;

        while (foundPlayers < currentRoom.PlayerCount)
        {
            if (currentRoom.Players.TryGetValue(actorIndex, out Photon.Realtime.Player foundPlayer))
            {
                if (foundPlayer == player)
                {
                    index = foundPlayers;
                    return true;
                }

                foundPlayers++;
            }

            actorIndex++;
        }

        index = -1;
        return false;
    }

    public string ParticipantName(int participantIndex) => "Participant" + (char)('A' + participantIndex);

    public void SetRoomPropParticipantID(int participantIndex, string userID)
    {
        ExitGames.Client.Photon.Hashtable property = new ExitGames.Client.Photon.Hashtable();
        property.Add(ParticipantName(participantIndex), userID);

        PhotonNetwork.CurrentRoom.SetCustomProperties(property);
    }

    public void SetPlayerPropParticipantIndex(int participantIndex)
    {
        ExitGames.Client.Photon.Hashtable property = new ExitGames.Client.Photon.Hashtable();
        property.Add(PlayerPropParticipantIndex, participantIndex);

        PhotonNetwork.LocalPlayer.SetCustomProperties(property);
    }

    #region Main Menu
    void SetPlayerName(string name)
    {
        PlayerPrefs.SetString(PlayerPrefsNameKey, name);
        PlayerPrefs.Save();

        PhotonNetwork.NickName = name;
        OnPlayerNameSet?.Invoke(name);
    }

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

        if (PhotonNetwork.IsConnectedAndReady) PhotonNetwork.CreateRoom(roomName, defaultRoomOptions);
        else ConnectToPhoton();
    }

    void BeginMatch()
    {
        bool playingAsPA = (int)PhotonNetwork.LocalPlayer.CustomProperties[PlayerPropParticipantIndex] == 0;
        OnMatchBegun?.Invoke(playingAsPA);

        if (PhotonNetwork.IsMasterClient) LoadScene(GameManager.Get().GameplayScene);
    }

    public void LoadScene(string sceneName)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (!loadingScene)
            {
                loadingScene = true;
                PhotonNetwork.LoadLevel(sceneName);
            }
            else Debug.LogError("Can not load scene: a scene is already being loaded");
        }
        else Debug.LogError("Can not load scene: client is not master");
    }

    public void Disconnect()
    {
        if (PhotonNetwork.InRoom)
        {
            if (currentRoom.PlayerCount > 1) PhotonNetwork.LeaveRoom();
            else
            {
                Debug.Log("Closing room...");
                currentRoom.IsOpen = false;
            }
        }

        if (PhotonNetwork.IsConnected) PhotonNetwork.Disconnect();
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
        else if (creatingNewRoom) PhotonNetwork.CreateRoom(handledRoomName, defaultRoomOptions);
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
        currentRoom = PhotonNetwork.CurrentRoom;

        for (int i = 0; i < currentRoom.PlayerCount; i++)
        {
            Photon.Realtime.Player player;
            if (GetPlayerByIndex(i, out player)) PlayersByID.Add(player.UserId, player);
        }

        OnRoomJoined?.Invoke();
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Leaving room...");

        currentRoom = null;
        PlayersByID.Clear();
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer) => PlayersByID.Add(newPlayer.UserId, newPlayer);

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) => PlayersByID.Remove(otherPlayer.UserId);

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