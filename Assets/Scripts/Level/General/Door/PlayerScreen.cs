using UnityEngine;
using UnityEngine.UI;

public class PlayerScreen : MonoBehaviour
{
    [SerializeField] bool on = false;
    [SerializeField] Text playerNameText = null;

    [Header("Rendering")]
    [SerializeField] Renderer screenRenderer = null;
    [SerializeField] Material onMaterial = null;
    [SerializeField] Material offMaterial = null;

    public bool On
    {
        set
        {
            on = value;
            screenRenderer.material = on ? onMaterial : offMaterial;
        }

        get { return on; }
    }

    public string PlayerName { set { playerNameText.text = value; } get { return playerNameText.text; } }

    void Start() => On = on;
}