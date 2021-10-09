using System;
using UnityEngine;

public class UIManager_MainMenu : MonoBehaviour
{
    [Header("Menues")]
    [SerializeField] GameObject lobby = null;
    [Space]
    [SerializeField] GameObject defaultMenu = null;
    GameObject currentMenu;

    static public event Action OnExit;

    void OnEnable() => NetworkManager.OnEnterLobby += EnterLobby;

    void Start() => currentMenu = defaultMenu;

    void OnDisable() => NetworkManager.OnEnterLobby -= EnterLobby;

    void EnterLobby() => GoToMenu(lobby);

    public void Exit() => OnExit?.Invoke();

    public void GoToMenu(GameObject targetMenu)
    {
        currentMenu.SetActive(false);
        targetMenu.SetActive(true);

        currentMenu = targetMenu;
    }
}