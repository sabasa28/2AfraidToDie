using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager_MainMenu : MonoBehaviour
{
    [SerializeField] GameObject margins = null;

    [Header("Text fields")]
    [SerializeField] TMP_Text versionText = null;
    [SerializeField] TMP_Text nameText = null;

    [Header("Buttons")]
    [SerializeField] Button returnButton = null;

    [Header("Menues")]
    [SerializeField] Menu defaultMenu = null;
    [Space]
    [SerializeField] Menu titleScreen = null;
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
        TitleScreen.OnTitleScreenClosed += () => titleScreen.gameObject.SetActive(false);

        NetworkManager.OnPlayerNameSet += OnPlayerNameSet;
        NetworkManager.OnRoomJoined += DisplayLobby;
        NetworkManager.OnFail += NotifyFail;

        Networking_Lobby.OnDisconnectedOnRoomClosed += DisplayRoomOptionsMenu;
    }

    void Start()
    {
        currentMenu = defaultMenu;
        if (!currentMenu.gameObject.activeInHierarchy) currentMenu.gameObject.SetActive(true);

        versionText.text = "v" + Application.version;
    }

    void OnDisable()
    {
        TitleScreen.OnTitleScreenClosed -= () => titleScreen.gameObject.SetActive(false);

        NetworkManager.OnPlayerNameSet -= OnPlayerNameSet;
        NetworkManager.OnRoomJoined -= DisplayLobby;
        NetworkManager.OnFail -= NotifyFail;

        Networking_Lobby.OnDisconnectedOnRoomClosed -= DisplayRoomOptionsMenu;
    }

    void DisplayRoomOptionsMenu() => DisplayMenu(roomOptionsMenu);

    void DisplayLobby() => DisplayMenu(lobby);

    void ActivateMenuInHierarchy(Menu targetMenu)
    {
        if (targetMenu.ParentMenu)
        {
            targetMenu.ParentMenu.gameObject.SetActive(true);

            if (!targetMenu.ParentMenu.gameObject.activeInHierarchy) ActivateMenuInHierarchy(targetMenu.ParentMenu);
        }
    }

    void OnPlayerNameSet(string name)
    {
        nameText.text = nameDisplayPreText + name;
        DisplayMenu(rootMenu);
    }

    void SetUpReturnButton(Menu targetMenu)
    {
        returnButton.onClick.RemoveAllListeners();
        returnButton.onClick.AddListener(() => { DisplayMenu(targetMenu); });

        returnButton.gameObject.SetActive(true);
    }

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

    public void DisplayMenu(Menu targetMenu)
    {
        currentMenu.gameObject.SetActive(false);
        targetMenu.gameObject.SetActive(true);

        if (!targetMenu.gameObject.activeInHierarchy) ActivateMenuInHierarchy(targetMenu);

        currentMenu = targetMenu;

        margins.SetActive(targetMenu.DisplayMargins);

        if (targetMenu.PreviousMenu != null) SetUpReturnButton(targetMenu.PreviousMenu);
        else returnButton.gameObject.SetActive(false);
    }

    public void Exit() => OnExit?.Invoke();
}