using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Networking_Lobby : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_Text roomNameText = null;
    [SerializeField] TMP_Text[] playerTexts = null;
    [SerializeField] Toggle participantAToggle = null;
    [SerializeField] Toggle participantBToggle = null;

    public override void OnEnable()
    {
        base.OnEnable();

        SetUp();
    }

    void SetUp()
    {
        Room room = PhotonNetwork.CurrentRoom;

        roomNameText.text = "Room \"" + room.Name + "\"";

        for (int i = 0; i < room.PlayerCount; i++)
            if (room.Players.TryGetValue(i + 1, out Photon.Realtime.Player player)) playerTexts[i].text = player.NickName;
    }

    void UpdatePlayerNames()
    {
        Room room = PhotonNetwork.CurrentRoom;

        for (int i = 0; i < room.PlayerCount; i++)
        {
            if (room.Players.TryGetValue(i + 1, out Photon.Realtime.Player player) && player.NickName != playerTexts[i].text) playerTexts[i].text = player.NickName;
        }
    }

    [PunRPC]
    void SetParticipant(bool settingA, string playerName)
    {
        Hashtable property = new Hashtable();
        string participant = settingA ? "Participant A" : "Participant B";
        property.Add(participant, playerName);

        PhotonNetwork.CurrentRoom.SetCustomProperties(property);
    }

    public void ChooseParticipant(bool chooseA)
    {
        string playerName = "";
        if (chooseA)
        {
            if (participantAToggle.isOn) playerName = PhotonNetwork.LocalPlayer.NickName;
            else playerName = "None";
        }
        else
        {
            if (participantBToggle.isOn) playerName = PhotonNetwork.LocalPlayer.NickName;
            else playerName = "None";
        }

        photonView.RPC("SetParticipant", RpcTarget.All, chooseA, playerName);
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("Participant A"))
        {
            if ((string)propertiesThatChanged["Participant A"] == "None") participantAToggle.interactable = true;
            else if ((string)propertiesThatChanged["Participant A"] != PhotonNetwork.LocalPlayer.NickName)
            {
                participantAToggle.interactable = false;
                participantBToggle.interactable = true;
            }
        }

        if (propertiesThatChanged.ContainsKey("Participant B"))
        {
            if ((string)propertiesThatChanged["Participant B"] == "None") participantBToggle.interactable = true;
            else if ((string)propertiesThatChanged["Participant B"] != PhotonNetwork.LocalPlayer.NickName)
            {
                participantAToggle.interactable = true;
                participantBToggle.interactable = false;
            }
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer) => UpdatePlayerNames();
}