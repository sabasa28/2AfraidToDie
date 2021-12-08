using ExitGames.Client.Photon;
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

    [Header("Messages")]
    [SerializeField] string createRoomTitle = "";
    [SerializeField] string createRoomMessage = "";
    [SerializeField] string createRoomFailTitle = "";
    [SerializeField] string existentRoomFailMessage = "";
    [Space]
    [SerializeField] string joinRoomTitle = "";
    [SerializeField] string joinRoomMessage = "";
    [SerializeField] string joinRoomFailTitle = "";
    [SerializeField] string inexistentRoomFailMessage = "";
    Dialog roomHandlingDialog;

    bool joiningRoom = false;
    bool settingUpParticipantIndex = false;
    bool creatingNewRoom = false;
    bool loadingScene = false;
    bool leavingRoom = false;
    bool disconnecting = false;

    string gameVersion;
    string handledRoomName;
    GameManager gameManager;
    DialogManager dialogManager;

    Room currentRoom;
    RoomOptions defaultRoomOptions;

    List<string> playerPropsBeingSet = new List<string>();
    List<string> roomPropsBeingSet = new List<string>();
    public Dictionary<string, Photon.Realtime.Player> PlayersByID { private set; get; } = new Dictionary<string, Photon.Realtime.Player>();
    
    const int MaxPlayersPerRoom = 2;

    public int PlayerCount { get { return currentRoom.PlayerCount; } }
    static public string PlayerPropParticipantIndex { set; get; } = "ParticipantIndex";
    static public string PlayerPrefsNameKey { private set; get; } = "PlayerName";

    static public event Action<bool> OnNamePlayerPrefNotSet;
    static public event Action<string> OnPlayerNameSet;
    static public event Action OnRoomJoined;
    static public event Action<bool> OnMatchBegun;

    public override void Awake()
    {
        base.Awake();

        gameVersion = Application.version;
        defaultRoomOptions = new RoomOptions{ PublishUserId = true, MaxPlayers = MaxPlayersPerRoom };
        gameManager = GameManager.Get();
        dialogManager = DialogManager.Get();

        PhotonNetwork.AutomaticallySyncScene = true;
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

    #region Photon
    #region Server Connection
    void ConnectToPhoton()
    {
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();
    }

    public void Disconnect()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Can not disconnect: client is already disconnected");
            return;
        }

        if (playerPropsBeingSet.Count == 0 && roomPropsBeingSet.Count == 0)
        {
            if (PhotonNetwork.InRoom)
            {
                if (currentRoom.PlayerCount > 1)
                {
                    Debug.Log("Leaving room...");
                    PhotonNetwork.LeaveRoom();
                }
                else
                {
                    Debug.Log("Closing room...");
                    currentRoom.IsOpen = false;
                }
            }

            PhotonNetwork.Disconnect();
        }

        if (!disconnecting)
        {
            disconnecting = true;
            Debug.Log("Disconnecting...");
        }
    }
    #endregion

    #region Room Handling
    void JoinRoom(string roomName)
    {
        if (!roomHandlingDialog) roomHandlingDialog = dialogManager.DisplayButtonlessMessageDialog(joinRoomTitle, joinRoomMessage);

        handledRoomName = roomName;
        joiningRoom = true;

        if (PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log("Joining room...");
            PhotonNetwork.JoinRoom(roomName);
        }
        else ConnectToPhoton();
    }

    void CreateNewRoom(string roomName)
    {
        if (!roomHandlingDialog) roomHandlingDialog = dialogManager.DisplayButtonlessMessageDialog(createRoomTitle, createRoomMessage);

        handledRoomName = roomName;
        creatingNewRoom = true;

        if (PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log("Creating new room...");
            PhotonNetwork.CreateRoom(roomName, defaultRoomOptions);
        }
        else ConnectToPhoton();
    }

    public void LeaveRoom()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Can not leave room: client is not connected");
            return;
        }
        else if (!PhotonNetwork.InRoom)
        {
            Debug.Log("Can not leave room: client is not in a room");
            return;
        }

        if (playerPropsBeingSet.Count == 0 && roomPropsBeingSet.Count == 0) PhotonNetwork.LeaveRoom();

        if (!leavingRoom)
        {
            leavingRoom = true;
            Debug.Log("Leaving room...");
        }
    }
    #endregion

    #region Properties
    public void SetPlayerPropParticipantIndex(int participantIndex)
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Can not set property " + PlayerPropParticipantIndex + ": client is not connected");
            return;
        }
        else if (playerPropsBeingSet.Contains(PlayerPropParticipantIndex))
        {
            Debug.Log("Can not set property " + PlayerPropParticipantIndex + ": property is already being set");
            return;
        }

        Hashtable property = new Hashtable();
        property.Add(PlayerPropParticipantIndex, participantIndex);

        PhotonNetwork.LocalPlayer.SetCustomProperties(property);
        playerPropsBeingSet.Add(PlayerPropParticipantIndex);
    }

    public void SetRoomPropParticipantID(int participantIndex, string userID)
    {
        if (!ParticipantName(participantIndex, out string participantName))
        {
            Debug.Log("Can not set property: participant index is out of range");
            return;
        }
        string key = participantName;

        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Can not set property " + key + ": client is not connected");
            return;
        }
        else if (roomPropsBeingSet.Contains(key))
        {
            Debug.Log("Can not set property " + key + ": property is already being set");
            return;
        }

        Hashtable property = new Hashtable();
        property.Add(key, userID);

        PhotonNetwork.CurrentRoom.SetCustomProperties(property);
        roomPropsBeingSet.Add(key);
    }
    #endregion

    #region Overrides
    #region Server Connection
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        Debug.Log("Connected to Master");

        if (joiningRoom) JoinRoom(handledRoomName); //PhotonNetwork.JoinRoom(handledRoomName);
        else if (creatingNewRoom) CreateNewRoom(handledRoomName); //PhotonNetwork.CreateRoom(handledRoomName, defaultRoomOptions);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);

        Debug.LogWarning($"Disconnected due to: { cause }");
        disconnecting = false;
    }
    #endregion

    #region Room Handling
    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();

        Debug.Log("Client successfully created a new room");

        creatingNewRoom = false;
        if (roomHandlingDialog) dialogManager.CloseDialog(roomHandlingDialog);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        Debug.Log("Client successfully joined a room");

        joiningRoom = false;
        currentRoom = PhotonNetwork.CurrentRoom;
        if (roomHandlingDialog) dialogManager.CloseDialog(roomHandlingDialog);

        SetPlayerPropParticipantIndex(-1);
        settingUpParticipantIndex = true;

        for (int i = 0; i < currentRoom.PlayerCount; i++)
        {
            Photon.Realtime.Player player;
            if (GetPlayerByIndex(i, out player)) PlayersByID.Add(player.UserId, player);
        }
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();

        Debug.Log("Client left room");

        leavingRoom = false;
        currentRoom = null;
        PlayersByID.Clear();

        if (SceneManager.GetActiveScene().name == gameManager.GameplayScene) LoadScene(gameManager.MainMenuScene);
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        PlayersByID.Add(newPlayer.UserId, newPlayer);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        SetRoomPropParticipantID((int)otherPlayer.CustomProperties[PlayerPropParticipantIndex], "");
        PlayersByID.Remove(otherPlayer.UserId);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);

        Debug.LogError($"Create room failed (code { returnCode }): { message }");

        dialogManager.CloseDialog(roomHandlingDialog);

        if (message == "A game with the specified id already exist.") message = existentRoomFailMessage;
        NotifyFail(FailTypes.CreateRoomFail, message);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);

        Debug.LogError($"Join room failed (code { returnCode }): { message }");

        dialogManager.CloseDialog(roomHandlingDialog);

        if (message == "Game does not exist") message = inexistentRoomFailMessage;
        NotifyFail(FailTypes.JoinRoomFail, message);
    }

    void NotifyFail(FailTypes failType, string failMessage)
    {
        string title = "";
        switch (failType)
        {
            case FailTypes.CreateRoomFail:
                title = createRoomFailTitle;
                break;
            case FailTypes.JoinRoomFail:
                title = joinRoomFailTitle;
                break;
            default: break;
        }

        DialogManager.Get().DisplayMessageDialog(title, failMessage, null, null);
    }
    #endregion

    #region Properties
    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

        if (targetPlayer.IsLocal)
        {
            bool propWasBeingSet = false;
            List<string> propsToRemove = new List<string>();

            foreach (string key in playerPropsBeingSet)
            {
                if (changedProps.ContainsKey(key))
                {
                    propWasBeingSet = true;
                    propsToRemove.Add(key);
                }
            }

            if (propsToRemove.Count > 0)
                foreach (string key in propsToRemove) playerPropsBeingSet.Remove(key);

            if (propWasBeingSet)
            {
                if (disconnecting) Disconnect();
                else if (leavingRoom) LeaveRoom();
                else if (settingUpParticipantIndex && propsToRemove.Contains(PlayerPropParticipantIndex))
                {
                    settingUpParticipantIndex = false;
                    OnRoomJoined?.Invoke();
                }
            }
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);

        bool propWasBeingSet = false;
        List<string> propsToRemove = new List<string>();

        foreach (string key in roomPropsBeingSet)
        {
            if (propertiesThatChanged.ContainsKey(key))
            {
                propWasBeingSet = true;
                propsToRemove.Add(key);
            }
        }

        if (propsToRemove.Count > 0)
            foreach (string key in propsToRemove) roomPropsBeingSet.Remove(key);

        if (propWasBeingSet)
        {
            if (disconnecting) Disconnect();
            else if (leavingRoom) LeaveRoom();
        }
    }
    #endregion
    #endregion
    #endregion

    #region Player Data
    void SetPlayerName(string name)
    {
        PlayerPrefs.SetString(PlayerPrefsNameKey, name);
        PlayerPrefs.Save();

        PhotonNetwork.NickName = name;
        OnPlayerNameSet?.Invoke(name);
    }

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
            bool propertySet = playerInRoom.CustomProperties.ContainsKey(PlayerPropParticipantIndex);
            if (propertySet && (int)playerInRoom.CustomProperties[PlayerPropParticipantIndex] == participantIndex)
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

    public bool ParticipantName(int participantIndex, out string participantName)
    {
        if (participantIndex >= 0 && participantIndex < currentRoom.MaxPlayers)
        {
            participantName = "Participant" + (char)('A' + participantIndex);
            return true;
        }
        else
        {
            participantName = "";
            return false;
        }
    }
    #endregion

    #region Scene Flow
    void BeginMatch()
    {
        bool playingAsPA = (int)PhotonNetwork.LocalPlayer.CustomProperties[PlayerPropParticipantIndex] == 0;
        OnMatchBegun?.Invoke(playingAsPA);

        if (PhotonNetwork.IsMasterClient) LoadScene(gameManager.GameplayScene);
    }

    public void LoadScene(string sceneName)
    {
        if (PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("Can not load scene: client is not master");
            return;
        }

        if (!loadingScene)
        {
            loadingScene = true;
            PhotonNetwork.LoadLevel(sceneName);
        }
        else Debug.LogError("Can not load scene: a scene is already being loaded");
    }

    void OnLevelLoaded(Scene scene, LoadSceneMode mode) => loadingScene = false;
    #endregion

    #region Gameplay
    public Player SpawnPlayer(Vector3 position, Quaternion rotation)
    {
        GameObject playerGO = PhotonNetwork.Instantiate(playerPrefab.name, position, rotation);
        return playerGO.GetComponent<Player>();
    }
    #endregion
}