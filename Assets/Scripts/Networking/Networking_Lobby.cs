using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Networking_Lobby : MonoBehaviourPunCallbacks
{
    [Header("Text fields")]
    [SerializeField] TMP_Text roomNameText = null;

    [Header("Buttons")]
    [SerializeField] Button disconnectButton = null;

    [Header("Toggles")]
    [SerializeField] GameObject playerConnectionTogglePrefab = null;
    [Space]
    [SerializeField] Toggle[] participantToggles = null;
    [SerializeField] RectTransform[] playerContainers = null;
    [SerializeField] RectTransform[] participantContainers = null;
    List<PlayerConnectionToggle> playerConnectionToggles;

    [Header("Timer")]
    [SerializeField] MatchCountdownTimer matchCountdownTimer = null;

    bool readyToBeginMatch = false;
    bool countingDown = false;

    NetworkManager networkManager;

    static public event Action OnMatchCountdownFinished;

    void Awake() => networkManager = NetworkManager.Get();

    public override void OnEnable()
    {
        base.OnEnable();

        NetworkManager.OnRoomJoined += UpdatePlayerTogglesOn;
        MatchCountdownTimer.OnCountdownFinished += OnCountdownFinished;

        SetUp();
    }

    public override void OnDisable()
    {
        base.OnDisable();

        NetworkManager.OnRoomJoined -= UpdatePlayerTogglesOn;
        MatchCountdownTimer.OnCountdownFinished -= OnCountdownFinished;

        disconnectButton.gameObject.SetActive(false);
    }

    void SetUp()
    {
        Room room = PhotonNetwork.CurrentRoom;

        //Texts
        roomNameText.text = "Show \"" + room.Name + "\"";

        //Toggles
        playerConnectionToggles = new List<PlayerConnectionToggle>();
        foreach (RectTransform playerContainer in playerContainers)
        {
            PlayerConnectionToggle newToggle = Instantiate(playerConnectionTogglePrefab).GetComponent<PlayerConnectionToggle>();
            newToggle.Initialize(playerContainer, participantContainers);

            playerConnectionToggles.Add(newToggle);
        }

        //Room properties
        if (room.PlayerCount == 1)
            for (int i = 0; i < room.MaxPlayers; i++) networkManager.SetRoomPropParticipantID(i, "");
        else
        {
            for (int i = 0; i < room.PlayerCount; i++)
            {
                networkManager.GetPlayerByIndex(i, out Photon.Realtime.Player player);
                bool propertySet = player.CustomProperties.ContainsKey(NetworkManager.PlayerPropParticipantIndex);
                int participantIndex = propertySet ? (int)player.CustomProperties[NetworkManager.PlayerPropParticipantIndex] : -1;

                if (!player.IsLocal && participantIndex > -1 && participantIndex < playerConnectionToggles.Count) participantToggles[participantIndex].interactable = false;
            }
        }
        UpdatePlayerTogglesOn();

        disconnectButton.gameObject.SetActive(true);
    }

    void UpdatePlayerTogglesOn()
    {
        for (int i = 0; i < playerConnectionToggles.Count; i++)
        {
            if (networkManager.GetPlayerByIndex(i, out Photon.Realtime.Player player)) playerConnectionToggles[i].TurnOn(player);
            else if (playerConnectionToggles[i].IsOn) playerConnectionToggles[i].TurnOff();
        }
    }

    public void SetParticipantUserID(int participantIndex)
    {
        string userID = participantToggles[participantIndex].isOn ? PhotonNetwork.LocalPlayer.UserId : "";
        networkManager.SetRoomPropParticipantID(participantIndex, userID);
    }

    public void SetPlayerParticipantIndex(int participantIndex)
    {
        int index = participantToggles[participantIndex].isOn ? -1 : participantIndex;
        networkManager.SetPlayerPropParticipantIndex(index);
    }

    #region Match Countdown
    void ProcessRoomPropChange()
    {
        bool allParticipantsSelected = true;

        for (int i = 0; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++)
        {
            networkManager.ParticipantName(i, out string participantName);
            string userID = (string)PhotonNetwork.CurrentRoom.CustomProperties[participantName];
            if (string.IsNullOrEmpty(userID))
            {
                allParticipantsSelected = false;
                participantToggles[i].interactable = true;

                if (countingDown) StopMatchCountdown();
            }
            else
            {
                Photon.Realtime.Player player = networkManager.PlayersByID[userID];
                if (!player.IsLocal) participantToggles[i].interactable = false;
            }
        }

        if (allParticipantsSelected) StartMatchCountdown();
    }

    void StartMatchCountdown()
    {
        readyToBeginMatch = true;

        matchCountdownTimer.StartCountdown();
        countingDown = true;
    }

    void StopMatchCountdown()
    {
        readyToBeginMatch = false;

        matchCountdownTimer.StopCountdown();
        countingDown = false;
    }

    void OnCountdownFinished() { if (readyToBeginMatch) OnMatchCountdownFinished?.Invoke(); }
    #endregion

    public void DisconnectFromLobby()
    {
        foreach (Toggle toggle in participantToggles)
        {
            toggle.isOn = false;
            toggle.interactable = true;
        }

        foreach (PlayerConnectionToggle toggle in playerConnectionToggles) Destroy(toggle.gameObject);
        playerConnectionToggles.Clear();

        networkManager.Disconnect();
    }

    #region Overrides
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);

        ProcessRoomPropChange();
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        if (!newPlayer.IsLocal) UpdatePlayerTogglesOn();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        if (!otherPlayer.IsLocal) UpdatePlayerTogglesOn();
    }
    #endregion
}