using ExitGames.Client.Photon;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerConnectionToggle : MonoBehaviourPunCallbacks
{
    [SerializeField] Toggle toggle = null;
    [SerializeField] Image image = null;
    [SerializeField] TMP_Text labelText = null;

    [Header("Sprites")]
    [SerializeField] Sprite onSprite = null;
    [SerializeField] Sprite offSprite = null;

    [Header("Font sizes")]
    [SerializeField] float offSize = 20.0f;

    [Header("Texts")]
    [SerializeField] string offText = "";

    RectTransform rect;

    Photon.Realtime.Player player;
    int participantIndex;

    RectTransform playerContainer;
    RectTransform[] participantContainers;

    public bool IsOn { get { return toggle.isOn; } }
    public Photon.Realtime.Player Player { get { return player; } }

    void Awake() => rect = GetComponent<RectTransform>();

    void OnValueChanged(bool value)
    {
        if (value)
        {
            image.sprite = onSprite;

            labelText.enableAutoSizing = true;
            labelText.text = player.NickName;
        }
        else
        {
            image.sprite = offSprite;

            labelText.enableAutoSizing = false;
            labelText.fontSize = offSize;
            labelText.text = offText;
        }
    }

    void CheckForUpdate(Hashtable changedProps)
    {
        if (!changedProps.ContainsKey(NetworkManager.PlayerPropParticipantIndex)) return;

        Debug.Log("PARTICIPANT OF PLAYER " + player.NickName + " CHANGED TO: " + (int)player.CustomProperties[NetworkManager.PlayerPropParticipantIndex]);
        MoveToContainer((int)player.CustomProperties[NetworkManager.PlayerPropParticipantIndex]);
    }

    void MoveToContainer(int participantIndex)
    {
        if (participantIndex < -1 || participantIndex >= PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            Debug.LogError("Can not move to container: participant index is out of range.");
            return;
        }

        if (player != null) Debug.Log("MOVING PLAYER " + player.NickName + " TO INDEX " + participantIndex);

        if (participantIndex == -1) rect.SetParent(playerContainer);
        else rect.SetParent(participantContainers[participantIndex]);

        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    public void Initialize(RectTransform _playerContainer, RectTransform[] _participantContainers)
    {
        playerContainer = _playerContainer;
        participantContainers = _participantContainers;

        TurnOff();
    }

    public void TurnOn(Photon.Realtime.Player connectedPlayer)
    {
        toggle.isOn = true;
        player = connectedPlayer;
        participantIndex = (int)player.CustomProperties[NetworkManager.PlayerPropParticipantIndex];

        Debug.Log("TURNING ON PLAYER " + player.NickName + " TO INDEX: " + participantIndex);
        MoveToContainer(participantIndex);

        OnValueChanged(toggle.isOn);
    }

    public void TurnOff()
    {
        toggle.isOn = false;
        player = null;
        participantIndex = -1;

        MoveToContainer(participantIndex);

        OnValueChanged(toggle.isOn);
    }

    #region Overrides
    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps) { if (targetPlayer == player) CheckForUpdate(changedProps); }
    #endregion
}