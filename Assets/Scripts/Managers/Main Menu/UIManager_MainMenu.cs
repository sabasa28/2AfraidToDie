using Photon.Pun;
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
    [SerializeField] Button changeNameButton = null;

    [Header("Menues")]
    [SerializeField] Menu titleScreen = null;
    [SerializeField] Menu rootMenu = null;
    [SerializeField] Menu roomOptionsMenu = null;
    [SerializeField] Menu lobby = null;

    [Header("Texts")]
    [SerializeField] string nameDisplayPreText = "Participante:     ";

    Menu currentMenu;

    static public event Action OnExit;

    void OnEnable()
    {
        TitleScreen.OnTitleScreenClosed += CloseTitleScreen;

        NetworkManager.OnPlayerNameSet += OnPlayerNameSet;
        NetworkManager.OnRoomJoined += DisplayLobby;
    }

    void Start()
    {
        versionText.text = "v" + Application.version;

        if (GameManager.Get().TitleScreenShown)
        {
            nameText.text = nameDisplayPreText + PhotonNetwork.LocalPlayer.NickName;
            DisplayMenu(rootMenu);
        }
        else DisplayMenu(titleScreen);
    }

    void OnDisable()
    {
        TitleScreen.OnTitleScreenClosed -= () => titleScreen.gameObject.SetActive(false);

        NetworkManager.OnPlayerNameSet -= OnPlayerNameSet;
        NetworkManager.OnRoomJoined -= DisplayLobby;
    }

    void CloseTitleScreen() { if (titleScreen) titleScreen.gameObject.SetActive(false); }

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

    public void DisplayMenu(Menu targetMenu)
    {
        if (currentMenu) currentMenu.gameObject.SetActive(false);
        targetMenu.gameObject.SetActive(true);
        currentMenu = targetMenu;

        if (!targetMenu.gameObject.activeInHierarchy) ActivateMenuInHierarchy(targetMenu);

        if (targetMenu.PreviousMenu != null) SetUpReturnButton(targetMenu.PreviousMenu);
        else returnButton.gameObject.SetActive(false);

        if (targetMenu == lobby) changeNameButton.gameObject.SetActive(false);
        else changeNameButton.gameObject.SetActive(true);

        margins.SetActive(targetMenu.DisplayMargins);
    }

    public void Exit() => OnExit?.Invoke();
}