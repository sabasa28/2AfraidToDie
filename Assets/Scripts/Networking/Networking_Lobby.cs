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
    [SerializeField] TMP_Text roomNameText = null;
    [SerializeField] TMP_Text matchCountdownText = null;

    [SerializeField] Button disconnectButton = null;

    [SerializeField] Toggle[] participantToggles = null;
    [SerializeField] PlayerConnectionToggle[] playerConnectionToggles = null;
    [SerializeField] RectTransform[] playerContainers = null;
    [SerializeField] RectTransform[] participantContainers = null;
    List<RectTransform> playerToggleTransforms;
    List<RectTransform> participantToggleTransforms;

    [Header("Lobby properties")]
    [SerializeField] int matchCountdownTimer = 5;

    bool readyToBeginMatch = false;
    bool countingDown = false;

    NetworkManager networkManager;

    static public event Action OnMatchCountdownFinished;
    static public event Action OnDisconnectedOnRoomClosed;

    void Awake() => networkManager = NetworkManager.Get();

    public override void OnEnable()
    {
        base.OnEnable();

        SetUp();
    }

    public override void OnDisable()
    {
        base.OnDisable();

        disconnectButton.gameObject.SetActive(false);
    }

    void SetUp()
    {
        Room room = PhotonNetwork.CurrentRoom;

        //Texts
        roomNameText.text = "Room \"" + room.Name + "\"";
        matchCountdownText.text = matchCountdownTimer.ToString();

        //Toggles
        playerToggleTransforms = new List<RectTransform>();
        participantToggleTransforms = new List<RectTransform>();
        foreach (PlayerConnectionToggle toggle in playerConnectionToggles) playerToggleTransforms.Add(toggle.transform as RectTransform);

        UpdatePlayerToggles();

        //Room properties
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
            for (int i = 0; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++) networkManager.SetRoomPropParticipantID(i, "");
        else
        {
            for (int i = 0; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++)
            {
                string key = networkManager.ParticipantName(i);
                networkManager.SetRoomPropParticipantID(i, (string)room.CustomProperties[key]);
            }
        }

        disconnectButton.gameObject.SetActive(true);
    }

    void UpdatePlayerToggles()
    {
        for (int i = 0; i < playerConnectionToggles.Length; i++)
        {
            Photon.Realtime.Player player;

            if (networkManager.PlayersByIndex.TryGetValue(i, out player))
            {
                if (!playerConnectionToggles[i].IsOn) playerConnectionToggles[i].TurnOn(player);
            }
            else if (playerConnectionToggles[i].IsOn) playerConnectionToggles[i].TurnOff();
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

                if (newPlayerIndex >= playerToggleTransforms.Count) playerToggleTransforms.Insert(newPlayerIndex, toggleTransform);
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

            if (newParticipantIndex >= participantToggleTransforms.Count) participantToggleTransforms.Insert(newParticipantIndex, toggleTransform);
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

                    int playerIndex = networkManager.GetPlayerIndex(player);
                    MoveToggleToPlayers(participantToggleTransforms[i], playerIndex);

                    participantToggles[i].interactable = true;
                }
                else
                {
                    string playerID = (string)propertiesThatChanged[key];
                    Photon.Realtime.Player player = networkManager.PlayersByID[playerID];

                    if (player.IsLocal) networkManager.SetPlayerPropParticipantIndex(i);
                    else participantToggles[i].interactable = false;

                    int playerIndex = networkManager.GetPlayerIndex(player);
                    MoveToggleToParticipants(playerToggleTransforms[playerIndex], i);

                    //for (int j = 0; j < PhotonNetwork.CurrentRoom.MaxPlayers; j++)
                    //{
                    //    if (j == i)
                    //    {
                    //        participantToggles[j].interactable = false;
                    //        continue;
                    //    }
                    //    else if ((string)PhotonNetwork.CurrentRoom.CustomProperties[ParticipantName(j)] == "") participantToggles[j].interactable = true;
                    //}
                }
            }

            if (allParticipantsSelected && (string)PhotonNetwork.CurrentRoom.CustomProperties[networkManager.ParticipantName(i)] == "")
            {
                allParticipantsSelected = false;
                if (countingDown) StopMatchCountdown();
            }
        }

        if (allParticipantsSelected) StartMatchCountdown();
    }

    void StartMatchCountdown()
    {
        matchCountdownText.gameObject.SetActive(true);
        readyToBeginMatch = true;

        StartCoroutine(MatchCountdown());
        countingDown = true;
    }

    void StopMatchCountdown()
    {
        matchCountdownText.gameObject.SetActive(false);
        readyToBeginMatch = false;
        countingDown = false;
    }

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

    #region Coroutines
    IEnumerator MatchCountdown()
    {
        int timer = matchCountdownTimer;

        while (readyToBeginMatch)
        {
            matchCountdownText.text = timer--.ToString();

            yield return new WaitForSeconds(1);

            if (timer <= 0)
            {
                matchCountdownText.text = "0";
                break;
            }
        }

        if (readyToBeginMatch) OnMatchCountdownFinished?.Invoke();
    }
    #endregion
}