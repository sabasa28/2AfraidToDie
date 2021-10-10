using System;
using UnityEngine;

public class UIManager_MainMenu : MonoBehaviour
{
    [Header("Menues")]
    [SerializeField] GameObject playerNameInputDialog = null;
    [SerializeField] GameObject lobby = null;
    [Space]
    [SerializeField] GameObject defaultMenu = null;

    GameObject currentMenu;

    static public event Action OnExit;

    void OnEnable()
    {
        NetworkManager.OnNamePlayerPrefNotSet += TurnPlayerNameInputDialogOn;
        NetworkManager.OnRoomJoined += GoToLobby;
    }

    void Start()
    {
        currentMenu = defaultMenu;
        if (!currentMenu.activeInHierarchy) currentMenu.SetActive(true);
    }

    void OnDisable()
    {
        NetworkManager.OnNamePlayerPrefNotSet -= TurnPlayerNameInputDialogOn;
        NetworkManager.OnRoomJoined -= GoToLobby;
    }

    void TurnPlayerNameInputDialogOn() => playerNameInputDialog.SetActive(true);

    void GoToLobby() => GoToMenu(lobby);

    public void Exit() => OnExit?.Invoke();

    public void GoToMenu(GameObject targetMenu)
    {
        currentMenu.SetActive(false);
        targetMenu.SetActive(true);

        currentMenu = targetMenu;
    }
}