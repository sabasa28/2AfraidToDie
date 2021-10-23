using Photon.Pun;
using System;
using UnityEngine;

public class Networking_PlayerNameInput : MonoBehaviourPunCallbacks
{
    [SerializeField] string namePromptMessage = "";

    string playerPrefsNameKey;
    PromptDialog playerNamePrompDialog;

    static public event Action<string> OnPlayerNameSaved;

    void Awake()
    {
        playerPrefsNameKey = NetworkManager.PlayerPrefsNameKey;

        if (!PlayerPrefs.HasKey(playerPrefsNameKey))
        {
            PlayerPrefs.SetString(playerPrefsNameKey, playerPrefsNameKey);
            PlayerPrefs.Save();
        }
    }

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

    //void PromptForPlayerName() => playerNameInputDialog.gameObject.SetActive(true);
    void PromptForPlayerName()
    {
        playerNamePrompDialog = DialogManager.Get().DisplayPromptDialog(namePromptMessage, null, SavePlayerName, null, null);
        playerNamePrompDialog.InputField.onValueChanged.AddListener(SetPlayerName);

        SetPlayerName(playerNamePrompDialog.InputField.text);
    }

    void SetPlayerName(string playerName) => playerNamePrompDialog.Buttons[DialogManager.ButtonType.Continue].Interactable = !string.IsNullOrEmpty(playerName);

    void SavePlayerName(string playerName)
    {
        PlayerPrefs.SetString(playerPrefsNameKey, playerName);
        PlayerPrefs.Save();

        OnPlayerNameSaved?.Invoke(playerName);
    }
}