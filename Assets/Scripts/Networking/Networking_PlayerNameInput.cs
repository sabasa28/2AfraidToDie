using Photon.Pun;
using System;
using UnityEngine;

public class Networking_PlayerNameInput : MonoBehaviourPunCallbacks
{
    [SerializeField] string newNamePromptTitle = "";
    [SerializeField] string nameChangePromptTitle = "";
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

    void SavePlayerName(string playerName) => OnPlayerNameSaved?.Invoke(playerName);

    public void PromptForPlayerName(bool firstTime)
    {
        string title = firstTime ? newNamePromptTitle : nameChangePromptTitle;
        playerNamePrompDialog = DialogManager.Get().DisplayPromptDialog(title, namePromptMessage, null, SavePlayerName, null, null);

        if (!firstTime) playerNamePrompDialog.InputField.text = PhotonNetwork.LocalPlayer.NickName;
    }
}