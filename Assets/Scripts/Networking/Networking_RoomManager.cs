using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Networking_RoomManager : MonoBehaviour
{
    [SerializeField] TMP_InputField roomNameInputField = null;
    [SerializeField] Button confirmRoomNameButton = null;

    static public event Action<string> OnJoiningRoom;
    static public event Action<string> OnCreatingNewRoom;

    void JoinRoom() => OnJoiningRoom?.Invoke(roomNameInputField.text);

    void CreateNewRoom() => OnCreatingNewRoom?.Invoke(roomNameInputField.text);

    public void OnEnteringRoomName(bool creatingNewRoom)
    {
        if (creatingNewRoom) confirmRoomNameButton.onClick.AddListener(CreateNewRoom);
        else confirmRoomNameButton.onClick.AddListener(JoinRoom);
    }
}