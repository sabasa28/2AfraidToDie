using Photon.Pun;
using System;
using UnityEngine;

public class Networking_PlayerNameInput : MonoBehaviourPunCallbacks
{
    [SerializeField] string namePromptMessage = "";

    PromptDialog playerNamePrompDialog;

    static public event Action<string> OnPlayerNameSaved;

    public override void OnEnable()
    {
        base.OnEnable();

        NetworkManager.OnNamePlayerPrefNotSet += PromptForPlayerName;
    }

    public override void OnDisable()
    {
        base.OnDisable();

        NetworkManager.OnNamePlayerPrefNotSet -= PromptForPlayerName;
    }

    void SetPlayerName(string playerName) => playerNamePrompDialog.Buttons[DialogManager.ButtonType.Continue].Interactable = !string.IsNullOrEmpty(playerName);

    void SavePlayerName(string playerName) => OnPlayerNameSaved?.Invoke(playerName);

    public void PromptForPlayerName()
    {
        playerNamePrompDialog = DialogManager.Get().DisplayPromptDialog(namePromptMessage, null, SavePlayerName, null, null);
        playerNamePrompDialog.InputField.onValueChanged.AddListener(SetPlayerName);

        SetPlayerName(playerNamePrompDialog.InputField.text);
    }
}