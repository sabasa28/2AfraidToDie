using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
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
    [SerializeField] Toggle[] participantToggles = null;
    [SerializeField] PlayerConnectionToggle[] playerConnectionToggles = null;
    [SerializeField] RectTransform[] playerContainers = null;
    [SerializeField] RectTransform[] participantContainers = null;
    List<RectTransform> playerToggleTransforms;
    List<RectTransform> participantToggleTransforms;

    [Header("Timer")]
    [SerializeField] MatchCountdownTimer matchCountdownTimer = null;

    bool readyToBeginMatch = false;
    bool countingDown = false;

    NetworkManager networkManager;

    static public event Action OnMatchCountdownFinished;
    static public event Action OnDisconnectedOnRoomClosed;

    void Awake() => networkManager = NetworkManager.Get();

    public override void OnEnable()
    {
        base.OnEnable();

        MatchCountdownTimer.OnCountdownFinished += OnCountdownFinished;

        SetUp();
    }

    public override void OnDisable()
    {
        base.OnDisable();

        MatchCountdownTimer.OnCountdownFinished -= OnCountdownFinished;

        disconnectButton.gameObject.SetActive(false);
    }

    void SetUp()
    {
        Room room = PhotonNetwork.CurrentRoom;

        //Texts
        roomNameText.text = "Show \"" + room.Name + "\"";

        //Toggles
        playerToggleTransforms = new List<RectTransform>();
        participantToggleTransforms = new List<RectTransform>();
        foreach (PlayerConnectionToggle toggle in playerConnectionToggles) if (toggle.TryGetComponent(out RectTransform rect)) playerToggleTransforms.Add(rect);

        //Room properties
        //if (room.PlayerCount == 1)
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
            for (int i = 0; i < room.MaxPlayers; i++) networkManager.SetRoomPropParticipantID(i, "");
        else
        {
            //for (int i = 0; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++)
            //{
            //    string key = networkManager.ParticipantName(i);
            //    networkManager.SetRoomPropParticipantID(i, (string)room.CustomProperties[key]);
            //}

            for (int j = 0; j < room.PlayerCount; j++)
            {
                networkManager.GetPlayerByIndex(j, out Photon.Realtime.Player player);
                int participantIndex = (int)player.CustomProperties[NetworkManager.PlayerPropParticipantIndex];

                if (participantIndex > -1)
                {
                    participantToggles[participantIndex].interactable = false;
                    MoveToggleToParticipants(playerToggleTransforms[j], participantIndex);
                }
            }
        }

        UpdatePlayerToggles();

        disconnectButton.gameObject.SetActive(true);
    }

    void UpdatePlayerToggles()
    {
        for (int i = 0; i < playerConnectionToggles.Length; i++)
        {
            if (networkManager.GetPlayerByIndex(i, out Photon.Realtime.Player player)) playerConnectionToggles[i].TurnOn(player);
            else playerConnectionToggles[i].TurnOff();
        }
    }

    void MoveToggleToPlayers(RectTransform toggleTransform, int newPlayerIndex)
    {
        if (!toggleTransform)
        {
            Debug.LogError("Toggle transform is null");
            return;
        }

        if (participantToggleTransforms.Contains(toggleTransform))
        {
            bool indexIsFree = false;
            if (newPlayerIndex >= playerToggleTransforms.Count) indexIsFree = true;
            else if (!playerToggleTransforms[newPlayerIndex]) indexIsFree = true;

            if (indexIsFree)
            {
                int oldIndex = participantToggleTransforms.IndexOf(toggleTransform);
                participantToggleTransforms[oldIndex] = null;

                if (newPlayerIndex >= playerToggleTransforms.Count)
                {
                    for (int i = 0; i <= newPlayerIndex; i++)
                    {
                        if (i != newPlayerIndex) playerToggleTransforms.Add(null);
                        else playerToggleTransforms.Add(toggleTransform);
                    }
                }
                else playerToggleTransforms[newPlayerIndex] = toggleTransform;

                toggleTransform.SetParent(playerContainers[newPlayerIndex]);
                toggleTransform.anchoredPosition = Vector2.zero;
            }
            else Debug.LogError("Player toggle space is already occupied");
        }
        else Debug.LogError("Passed transform is not a participant toggle transform");
    }

    void MoveToggleToParticipants(RectTransform toggleTransform, int newParticipantIndex)
    {
        if (!toggleTransform)
        {
            Debug.LogError("Toggle transform is null");
            return;
        }

        List<RectTransform> oldToggleTransforms;
        if (playerToggleTransforms.Contains(toggleTransform)) oldToggleTransforms = playerToggleTransforms;
        else if (participantToggleTransforms.Contains(toggleTransform)) oldToggleTransforms = participantToggleTransforms;
        else
        {
            Debug.LogError("Passed transform is not a toggle transform");
            return;
        }

        bool indexIsFree = false;
        if (newParticipantIndex >= participantToggleTransforms.Count) indexIsFree = true;
        else if (!participantToggleTransforms[newParticipantIndex]) indexIsFree = true;

        if (indexIsFree)
        {
            int oldIndex = oldToggleTransforms.IndexOf(toggleTransform);
            oldToggleTransforms[oldIndex] = null;

            if (newParticipantIndex >= participantToggleTransforms.Count)
            {
                for (int i = participantToggleTransforms.Count; i <= newParticipantIndex; i++)
                {
                    if (i != newParticipantIndex) participantToggleTransforms.Add(null);
                    else participantToggleTransforms.Add(toggleTransform);
                }
            }
            else participantToggleTransforms[newParticipantIndex] = toggleTransform;

            toggleTransform.SetParent(participantContainers[newParticipantIndex]);
            toggleTransform.anchoredPosition = Vector2.zero;
        }
        else Debug.LogError("Participant toggle space is already occupied");
    }

    void UpdateParticipantProperties(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        bool allParticipantsSelected = true;

        for (int i = 0; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++)
        {
            string key = networkManager.ParticipantName(i);

            if (propertiesThatChanged.ContainsKey(key))
            {
                if ((string)propertiesThatChanged[key] == "")
                {
                    Photon.Realtime.Player player;
                    if (!networkManager.GetPlayerByParticipantIndex(i, out player)) continue;

                    if (player.IsLocal) networkManager.SetPlayerPropParticipantIndex(-1);

                    networkManager.GetPlayerIndex(player, out int playerIndex);
                    MoveToggleToPlayers(participantToggleTransforms[i], playerIndex);

                    participantToggles[i].interactable = true;
                }
                else
                {
                    string playerID = (string)propertiesThatChanged[key];
                    Photon.Realtime.Player player = networkManager.PlayersByID[playerID];

                    if (player.IsLocal) networkManager.SetPlayerPropParticipantIndex(i);
                    else participantToggles[i].interactable = false;

                    networkManager.GetPlayerIndex(player, out int playerIndex);
                    MoveToggleToParticipants(playerToggleTransforms[playerIndex], i);
                }
            }

            if (allParticipantsSelected && string.IsNullOrEmpty((string)PhotonNetwork.CurrentRoom.CustomProperties[networkManager.ParticipantName(i)]))
            {
                allParticipantsSelected = false;
                if (countingDown) StopMatchCountdown();
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

    public void ChooseParticipant(int participantIndex)
    {
        string userID = "";
        if (participantToggles[participantIndex].isOn) userID = PhotonNetwork.LocalPlayer.UserId;

        networkManager.SetRoomPropParticipantID(participantIndex, userID);
    }

    public void DisconnectFromLobby() => networkManager.DisconnectFromRoom();

    #region Overrides
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged) => UpdateParticipantProperties(propertiesThatChanged);

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer) => UpdatePlayerToggles();

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) => UpdatePlayerToggles();
    #endregion
}