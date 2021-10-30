using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager_MainMenu : MonoBehaviour
{
    [SerializeField] GameObject margins = null;

    [Header("Text fields")]
    [SerializeField] TMP_Text nameText = null;

    [Header("Buttons")]
    [SerializeField] Button returnButton = null;

    [Header("Menues")]
    [SerializeField] Menu rootMenu = null;
    [SerializeField] Menu roomOptionsMenu = null;
    [SerializeField] Menu lobby = null;

    [Header("Texts")]
    [SerializeField] string createRoomFailPreText = "Error al crear show: ";
    [SerializeField] string joinRoomFailPreText = "Error al unirse a show: ";
    [SerializeField] string nameDisplayPreText = "Participante:     ";

    Menu currentMenu;

    static public event Action OnExit;

    void OnEnable()
    {
        NetworkManager.OnPlayerNameSet += UpdatePlayerName;
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
        NetworkManager.OnPlayerNameSet -= UpdatePlayerName;
        NetworkManager.OnRoomJoined -= GoToLobby;
        NetworkManager.OnFail -= NotifyFail;

        Networking_Lobby.OnDisconnectedOnRoomClosed -= GoToRoomOptionsMenu;
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
                message = createRoomFailPreText + failMessage;
                break;
            case NetworkManager.FailTypes.JoinRoomFail:
                message = joinRoomFailPreText + failMessage;
                break;
            default: break;
        }

        DialogManager.Get().DisplayMessageDialog(message, null, null);
    }

    void UpdatePlayerName(string name) => nameText.text = nameDisplayPreText + name;

    public void DisplayMenu(Menu targetMenu)
    {
        currentMenu.gameObject.SetActive(false);
        targetMenu.gameObject.SetActive(true);

        currentMenu = targetMenu;

        margins.SetActive(targetMenu.DisplayMargins);

        //Header
        //bool displayPlayerName = targetMenu == lobby ? false : true;
        //playerNameDisplay.SetActive(displayPlayerName);

        //Footer
        if (targetMenu.PreviousMenu != null) SetUpReturnButton(targetMenu.PreviousMenu);
        else returnButton.gameObject.SetActive(false);
    }

    public void Exit() => OnExit?.Invoke();
}