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
                int participantIndex = (int)player.CustomProperties[NetworkManager.PlayerPropParticipantIndex];

                if (participantIndex > -1 && participantIndex < playerConnectionToggles.Count)
                {
                    participantToggles[participantIndex].interactable = false;
                    playerConnectionToggles[i].TurnOn(player);
                }
            }
        }

        UpdatePlayerToggles();

        disconnectButton.gameObject.SetActive(true);
    }

    void UpdatePlayerToggles()
    {
        for (int i = 0; i < playerConnectionToggles.Count; i++)
        {
            if (networkManager.GetPlayerByIndex(i, out Photon.Realtime.Player player))
            {
                if (!playerConnectionToggles[i].IsOn) playerConnectionToggles[i].TurnOn(player);
            }
            else if (playerConnectionToggles[i].IsOn) playerConnectionToggles[i].TurnOff();
        }
    }

    //void MoveToggleToPlayers(RectTransform toggleTransform, int newPlayerIndex)
    //{
    //    if (!toggleTransform)
    //    {
    //        Debug.LogError("Toggle transform is null");
    //        return;
    //    }
    //
    //    if (participantToggleTransforms.Contains(toggleTransform))
    //    {
    //        bool indexIsFree = false;
    //        if (newPlayerIndex >= playerToggleTransforms.Count) indexIsFree = true;
    //        else if (!playerToggleTransforms[newPlayerIndex]) indexIsFree = true;
    //
    //        if (indexIsFree)
    //        {
    //            int oldIndex = participantToggleTransforms.IndexOf(toggleTransform);
    //            participantToggleTransforms[oldIndex] = null;
    //
    //            if (newPlayerIndex >= playerToggleTransforms.Count)
    //            {
    //                for (int i = 0; i <= newPlayerIndex; i++)
    //                {
    //                    if (i != newPlayerIndex) playerToggleTransforms.Add(null);
    //                    else playerToggleTransforms.Add(toggleTransform);
    //                }
    //            }
    //            else playerToggleTransforms[newPlayerIndex] = toggleTransform;
    //
    //            toggleTransform.SetParent(playerContainers[newPlayerIndex]);
    //            toggleTransform.anchoredPosition = Vector2.zero;
    //        }
    //        else Debug.LogError("Player toggle space is already occupied");
    //    }
    //    else Debug.LogError("Passed transform is not a participant toggle transform");
    //}

    //void MoveToggleToParticipants(RectTransform toggleTransform, int newParticipantIndex)
    //{
    //    if (!toggleTransform)
    //    {
    //        Debug.LogError("Toggle transform is null");
    //        return;
    //    }
    //
    //    List<RectTransform> oldToggleTransforms;
    //    if (playerToggleTransforms.Contains(toggleTransform)) oldToggleTransforms = playerToggleTransforms;
    //    else if (participantToggleTransforms.Contains(toggleTransform)) oldToggleTransforms = participantToggleTransforms;
    //    else
    //    {
    //        Debug.LogError("Passed transform is not a toggle transform");
    //        return;
    //    }
    //
    //    bool indexIsFree = false;
    //    if (newParticipantIndex >= participantToggleTransforms.Count) indexIsFree = true;
    //    else if (!participantToggleTransforms[newParticipantIndex]) indexIsFree = true;
    //
    //    if (indexIsFree)
    //    {
    //        int oldIndex = oldToggleTransforms.IndexOf(toggleTransform);
    //        oldToggleTransforms[oldIndex] = null;
    //
    //        if (newParticipantIndex >= participantToggleTransforms.Count)
    //        {
    //            for (int i = participantToggleTransforms.Count; i <= newParticipantIndex; i++)
    //            {
    //                if (i != newParticipantIndex) participantToggleTransforms.Add(null);
    //                else participantToggleTransforms.Add(toggleTransform);
    //            }
    //        }
    //        else participantToggleTransforms[newParticipantIndex] = toggleTransform;
    //
    //        toggleTransform.SetParent(participantContainers[newParticipantIndex]);
    //        toggleTransform.anchoredPosition = Vector2.zero;
    //    }
    //    else Debug.LogError("Participant toggle space is already occupied");
    //}

    void UpdateParticipantProperties(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        bool allParticipantsSelected = true;

        for (int i = 0; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++)
        {
            string key = networkManager.ParticipantName(i);
            if (propertiesThatChanged.ContainsKey(key))
            {
                if (string.IsNullOrEmpty((string)propertiesThatChanged[key]))
                {
                    Photon.Realtime.Player player;
                    if (networkManager.GetPlayerByParticipantIndex(i, out player) && player.IsLocal) networkManager.SetPlayerPropParticipantIndex(-1);
                    participantToggles[i].interactable = true;

                    allParticipantsSelected = false;
                    if (countingDown) StopMatchCountdown();
                    continue;

                    //networkManager.GetPlayerIndex(player, out int playerIndex);
                    //MoveToggleToPlayers(participantToggleTransforms[i], playerIndex);
                }
                else
                {
                    string playerID = (string)propertiesThatChanged[key];
                    Photon.Realtime.Player player = networkManager.PlayersByID[playerID];

                    if (player.IsLocal) networkManager.SetPlayerPropParticipantIndex(i);
                    else participantToggles[i].interactable = false;

                    //networkManager.GetPlayerIndex(player, out int playerIndex);
                    //MoveToggleToParticipants(playerToggleTransforms[playerIndex], i);
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

    public void DisconnectFromLobby()
    {
        foreach (Toggle toggle in participantToggles) toggle.isOn = false;
        foreach (Toggle toggle in participantToggles) toggle.interactable = true;

        foreach (PlayerConnectionToggle toggle in playerConnectionToggles) Destroy(toggle.gameObject);
        playerConnectionToggles.Clear();

        networkManager.Disconnect();
    }

    #region Overrides
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged) => UpdateParticipantProperties(propertiesThatChanged);

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer) => UpdatePlayerToggles();

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) => UpdatePlayerToggles();
    #endregion
}