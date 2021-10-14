using ExitGames.Client.Photon;
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

    [Header("Lobby properties")]
    [SerializeField] int matchCountdownTimer = 5;

    bool readyToBeginMatch = false;
    bool countingDown = false;

    static public event Action OnMatchCountdownFinished;

    public override void OnEnable()
    {
        base.OnEnable();

        SetUp();
    }

    void SetUp()
    {
        Room room = PhotonNetwork.CurrentRoom;

        roomNameText.text = "Room \"" + room.Name + "\"";
        matchCountdownText.text = matchCountdownTimer.ToString();

        for (int i = 0; i < room.PlayerCount; i++)
            if (room.Players.TryGetValue(i + 1, out Photon.Realtime.Player player)) playerTexts[i].text = player.NickName;

        ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
        for (int i = 0; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++) properties.Add(ParticipantName(i), "None");
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
    }

    void UpdatePlayerNames()
    {
        Room room = PhotonNetwork.CurrentRoom;

        for (int i = 0; i < room.PlayerCount; i++)
        {
            if (room.Players.TryGetValue(i + 1, out Photon.Realtime.Player player) && player.NickName != playerTexts[i].text) playerTexts[i].text = player.NickName;
        }
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

    [PunRPC]
    void SetParticipant(int participantIndex, string playerName)
    {
        Debug.Log("setting participant");
        
        ExitGames.Client.Photon.Hashtable property = new ExitGames.Client.Photon.Hashtable();
        property.Add(ParticipantName(participantIndex), playerName);

        PhotonNetwork.CurrentRoom.SetCustomProperties(property);
    }

    public void ChooseParticipant(int playerIndex)
    {
        string playerName = "";
        if (participantToggles[playerIndex].isOn) playerName = PhotonNetwork.LocalPlayer.NickName;
        else playerName = "None";

        photonView.RPC("SetParticipant", RpcTarget.All, playerIndex, playerName);
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
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

        if (allParticipantsSelected)
        {
            Debug.Log(PhotonNetwork.LocalPlayer.NickName);
            StartMatchCountdown();
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer) => UpdatePlayerNames();

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
}