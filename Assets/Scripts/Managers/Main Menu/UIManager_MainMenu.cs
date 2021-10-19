using System;
using UnityEngine;
using UnityEngine.UI;

public class UIManager_MainMenu : MonoBehaviour
{
    [Header("Menues")]
    [SerializeField] Menu rootMenu = null;
    [SerializeField] Menu lobby = null;

    [Header("Buttons")]
    [SerializeField] Button returnButton = null;

    [Header("Dialogs")]
    [SerializeField] Dialog playerNameInputDialog = null;
    [SerializeField] Dialog failNotificationDialog = null;

    Menu currentMenu;
    Dialog currentDialog;

    static public event Action OnExit;

    void OnEnable()
    {
        NetworkManager.OnNamePlayerPrefNotSet += TurnPlayerNameInputDialogOn;
        NetworkManager.OnRoomJoined += GoToLobby;
        NetworkManager.OnCreatingRoomFailed += NotifyCreatingRoomFail;
        NetworkManager.OnJoiningRoomFailed += NotifyJoinRoomFail;
    }

    void Start()
    {
        currentMenu = rootMenu;
        if (!currentMenu.gameObject.activeInHierarchy) currentMenu.gameObject.SetActive(true);
    }

    void OnDisable()
    {
        NetworkManager.OnNamePlayerPrefNotSet -= TurnPlayerNameInputDialogOn;
        NetworkManager.OnRoomJoined -= GoToLobby;
        NetworkManager.OnCreatingRoomFailed -= NotifyCreatingRoomFail;
        NetworkManager.OnJoiningRoomFailed -= NotifyJoinRoomFail;
    }

    void SetUpReturnButton(Menu targetMenu)
    {
        returnButton.onClick.RemoveAllListeners();
        returnButton.onClick.AddListener(() => { DisplayMenu(targetMenu); });

        returnButton.gameObject.SetActive(true);
    }

    void GoToLobby() => DisplayMenu(lobby);

    void NotifyCreatingRoomFail(string failMessage)
    {
        if (failNotificationDialog.gameObject.activeInHierarchy) return;

        failNotificationDialog.Message = "Create room failed: " + failMessage;
        DisplayMenu(failNotificationDialog);
    }

    void NotifyJoinRoomFail(string failMessage)
    {
        if (failNotificationDialog.gameObject.activeInHierarchy) return;

        failNotificationDialog.Message = "Join room failed: " + failMessage;
        DisplayMenu(failNotificationDialog);
    }

    void TurnPlayerNameInputDialogOn() => playerNameInputDialog.gameObject.SetActive(true);

    public void DisplayMenu(Menu targetMenu)
    {
        currentMenu.gameObject.SetActive(false);
        if (currentDialog) currentDialog.gameObject.SetActive(false);
        targetMenu.gameObject.SetActive(true);

        currentMenu = targetMenu;

        if (targetMenu.PreviousMenu != null) SetUpReturnButton(targetMenu.PreviousMenu);
        else returnButton.gameObject.SetActive(false);
    }

    public void DisplayDialog(Dialog targetDialog)
    {
        if (currentDialog) currentDialog.gameObject.SetActive(false);
        targetDialog.gameObject.SetActive(true);

        currentDialog = targetDialog;

        if (targetDialog.PreviousMenu != null) SetUpReturnButton(targetDialog.PreviousMenu);
        else returnButton.gameObject.SetActive(false);
    }

    public void Exit() => OnExit?.Invoke();
}