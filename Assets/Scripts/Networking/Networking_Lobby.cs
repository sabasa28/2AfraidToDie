using TMPro;
using UnityEngine;

public class Networking_Lobby : MonoBehaviour
{
    [SerializeField] TMP_Text roomNameText = null;
    [SerializeField] TMP_Text player1Text = null;
    [SerializeField] TMP_Text player2Text = null;

    void OnEnable()
    {
        NetworkManager.RoomData room = NetworkManager.CurrentRoom;

        roomNameText.text = "Room \"" + room.Name + "\"";

        if (room.PlayerCount > 0) player1Text.text = room.PlayerNames[0];
        if (room.PlayerCount > 1) player2Text.text = room.PlayerNames[1];
    }
}