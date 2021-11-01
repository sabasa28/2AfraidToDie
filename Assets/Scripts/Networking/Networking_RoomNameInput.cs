using System;
using UnityEngine;
using UnityEngine.Events;

public class Networking_RoomNameInput : MonoBehaviour
{
    [SerializeField] string namePromptTitle = "";
    [SerializeField] string namePromptMessage = "";

    static public event Action<string> OnJoiningRoom;
    static public event Action<string> OnCreatingNewRoom;

    void JoinRoom(string roomName) => OnJoiningRoom?.Invoke(roomName);

    void CreateNewRoom(string roomName) => OnCreatingNewRoom?.Invoke(roomName);

    public void OnEnteringRoomName(bool creatingNewRoom)
    {
        UnityAction<string> onContinue;
        if (creatingNewRoom) onContinue = CreateNewRoom;
        else onContinue = JoinRoom;

        DialogManager.Get().DisplayPromptDialog(namePromptTitle, namePromptMessage, null, onContinue, null, null);
    }
}