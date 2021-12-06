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

    bool joiningRoom = false;
    bool settingUpParticipantIndex = false;
    bool creatingNewRoom = false;
    bool loadingScene = false;
    bool disconnecting = false;

    string gameVersion;
    string handledRoomName;

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
    static public event Action<FailTypes, string> OnFail;
    static public event Action<bool> OnMatchBegun;

    public override void Awake()
    {
        base.Awake();

        gameVersion = Application.version;
        defaultRoomOptions = new RoomOptions{ PublishUserId = true, MaxPlayers = MaxPlayersPerRoom };

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

    #region Properties
    public void SetPlayerPropParticipantIndex(int participantIndex)
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Can not set property " + PlayerPropParticipantIndex + ": client is not connected");
            return;
        }

        if (playerPropsBeingSet.Contains(PlayerPropParticipantIndex))
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

        if (roomPropsBeingSet.Contains(key))
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

        disconnecting = true;
        Debug.Log("Disconnecting...");
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
    #region Server Connection
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        Debug.Log("Connected to Master");

        if (joiningRoom) PhotonNetwork.JoinRoom(handledRoomName);
        else if (creatingNewRoom) PhotonNetwork.CreateRoom(handledRoomName, defaultRoomOptions);
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
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        Debug.Log("Client successfully joined a room");

        joiningRoom = false;
        currentRoom = PhotonNetwork.CurrentRoom;

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

        currentRoom = null;
        PlayersByID.Clear();
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

        OnFail?.Invoke(FailTypes.CreateRoomFail, message);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);

        Debug.LogError($"Join room failed (code { returnCode }): { message }");

        OnFail?.Invoke(FailTypes.JoinRoomFail, message);
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
                if (disconnecting) PhotonNetwork.Disconnect();
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

        if (propWasBeingSet && disconnecting) PhotonNetwork.Disconnect();
    }
    #endregion
    #endregion
}