using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Networking_Lobby : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_Text roomNameText = null;
    [SerializeField] TMP_Text matchCountdownText = null;
    [SerializeField] TMP_Text[] playerTexts = null;
    [SerializeField] Toggle[] participantToggles = null;
    [SerializeField] Button disconnectButton = null;

    [Header("Texts")]
    [SerializeField] string emptyPlayerText = "Waiting for player...";
    [SerializeField] string masterDisconnectMessage = "Master disconnected. Closing lobby...";

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

        roomNameText.text = "Room \"" + room.Name + "\"";
        matchCountdownText.text = matchCountdownTimer.ToString();

        UpdatePlayerNames();

        ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            for (int i = 0; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++) properties.Add(ParticipantName(i), "None");
            PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
        }
        else
        {
            for (int i = 0; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++)
            {
                string key = ParticipantName(i);
                string value = (string)room.CustomProperties[ParticipantName(i)];

                properties.Add(key, value);
            }
            UpdateParticipantProperties(properties);
        }

        disconnectButton.gameObject.SetActive(true);
    }

    void UpdatePlayerNames()
    {
        Room room = PhotonNetwork.CurrentRoom;

        for (int i = 0; i < room.MaxPlayers; i++)
        {
            if (room.Players.TryGetValue(i + 1, out Photon.Realtime.Player player)) playerTexts[i].text = player.NickName;
            else playerTexts[i].text = emptyPlayerText;
        }
    }

    void UpdateParticipantProperties(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        bool allParticipantsSelected = true;

        for (int i = 0; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++)
        {
            string key = ParticipantName(i);

            if (propertiesThatChanged.ContainsKey(key))
            {
                if ((string)propertiesThatChanged[key] == "None") participantToggles[i].interactable = true;
                else if ((string)propertiesThatChanged[key] != PhotonNetwork.LocalPlayer.NickName)
                {
                    for (int j = 0; j < PhotonNetwork.CurrentRoom.MaxPlayers; j++)
                    {
                        if (j == i)
                        {
                            participantToggles[j].interactable = false;
                            continue;
                        }
                        else if ((string)PhotonNetwork.CurrentRoom.CustomProperties[ParticipantName(j)] == "None") participantToggles[j].interactable = true;
                    }
                }
            }

            if (allParticipantsSelected && (string)PhotonNetwork.CurrentRoom.CustomProperties[ParticipantName(i)] == "None")
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

    string ParticipantName(int participantIndex) { return "Participant " + (char)('A' + participantIndex); }

    public void ChooseParticipant(int playerIndex)
    {
        string playerName = "";
        if (participantToggles[playerIndex].isOn) playerName = PhotonNetwork.LocalPlayer.NickName;
        else playerName = "None";

        photonView.RPC("SetParticipant", RpcTarget.All, playerIndex, playerName);
    }

    public void DisconnectFromLobby()
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient) photonView.RPC("DisconnectOnRoomClosed", RpcTarget.Others);

        networkManager.DisconnectFromRoom();
    }

    #region RPCs
    [PunRPC]
    void SetParticipant(int participantIndex, string playerName)
    {
        ExitGames.Client.Photon.Hashtable property = new ExitGames.Client.Photon.Hashtable();
        property.Add(ParticipantName(participantIndex), playerName);

        PhotonNetwork.CurrentRoom.SetCustomProperties(property);
    }

    [PunRPC]
    void DisconnectOnRoomClosed() => DialogManager.Get().DisplayMessageDialog(masterDisconnectMessage, null, () => OnDisconnectedOnRoomClosed?.Invoke());
    #endregion

    #region Overrides
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged) => UpdateParticipantProperties(propertiesThatChanged);

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer) => UpdatePlayerNames();

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) => UpdatePlayerNames();
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