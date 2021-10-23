using System;
using UnityEngine;
using UnityEngine.UI;

public class UIManager_MainMenu : MonoBehaviour
{
    [Header("Menues")]
    [SerializeField] Menu rootMenu = null;
    [SerializeField] Menu roomOptionsMenu = null;
    [SerializeField] Menu lobby = null;

    [Header("Buttons")]
    [SerializeField] Button returnButton = null;

    Menu currentMenu;

    static public event Action OnExit;

    void OnEnable()
    {
        NetworkManager.OnRoomJoined += GoToLobby;
        NetworkManager.OnFail += NotifyFail;

        Networking_Lobby.OnDisconnectedOnRoomClosed += GoToRoomOptionsMenu;
    }

    void Start()
    {
        currentMenu = rootMenu;
        if (!currentMenu.gameObject.activeInHierarchy) currentMenu.gameObject.SetActive(true);
    }

    void OnDisable()
    {
        NetworkManager.OnRoomJoined -= GoToLobby;
        NetworkManager.OnFail -= NotifyFail;

        Networking_Lobby.OnDisconnectedOnRoomClosed += GoToRoomOptionsMenu;
    }

    void SetUpReturnButton(Menu targetMenu)
    {
        returnButton.onClick.RemoveAllListeners();
        returnButton.onClick.AddListener(() => { DisplayMenu(targetMenu); });

        returnButton.gameObject.SetActive(true);
    }

    void GoToRoomOptionsMenu() => DisplayMenu(roomOptionsMenu);

    void GoToLobby() => DisplayMenu(lobby);

    void NotifyFail(NetworkManager.FailTypes failType, string failMessage)
    {
        string message = "";
        switch (failType)
        {
            case NetworkManager.FailTypes.CreateRoomFail:
                message = "Create room failed: " + failMessage;
                break;
            case NetworkManager.FailTypes.JoinRoomFail:
                message = "Join room failed: " + failMessage;
                break;
            default: break;
        }

        DialogManager.Get().DisplayMessageDialog(message, null, null);
    }

    public void DisplayMenu(Menu targetMenu)
    {
        currentMenu.gameObject.SetActive(false);
        targetMenu.gameObject.SetActive(true);

        currentMenu = targetMenu;

        if (targetMenu.PreviousMenu != null) SetUpReturnButton(targetMenu.PreviousMenu);
        else returnButton.gameObject.SetActive(false);
    }

    public void Exit() => OnExit?.Invoke();
}