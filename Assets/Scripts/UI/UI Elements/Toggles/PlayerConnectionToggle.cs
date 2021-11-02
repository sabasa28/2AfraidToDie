using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerConnectionToggle : MonoBehaviour
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

    Photon.Realtime.Player player;

    public bool IsOn { get { return toggle.isOn; } }
    public Photon.Realtime.Player Player { get { return player; } }

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

    public void TurnOn(Photon.Realtime.Player connectedPlayer)
    {
        toggle.isOn = true;
        player = connectedPlayer;

        OnValueChanged(toggle.isOn);
    }

    public void TurnOff()
    {
        toggle.isOn = false;
        player = null;

        OnValueChanged(toggle.isOn);
    }
}