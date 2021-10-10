using Photon.Pun;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Networking_PlayerNameInput : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_InputField playerNameInputField = null;
    [SerializeField] Button confirmButton = null;

    string playerPrefsNameKey;

    static public event Action<string> OnPlayerNameSaved;

    void Awake()
    {
        playerPrefsNameKey = NetworkManager.PlayerPrefsNameKey;

        if (!PlayerPrefs.HasKey(playerPrefsNameKey))
        {
            PlayerPrefs.SetString(playerPrefsNameKey, playerPrefsNameKey);
            PlayerPrefs.Save();
        }

        SetPlayerName();
    }

    public void SetPlayerName() => confirmButton.interactable = !string.IsNullOrEmpty(playerNameInputField.text);

    public void SavePlayerName()
    {
        string playerName = playerNameInputField.text;

        PlayerPrefs.SetString(playerPrefsNameKey, playerName);
        PlayerPrefs.Save();

        OnPlayerNameSaved?.Invoke(playerName);
    }
}