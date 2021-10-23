using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogButton : MonoBehaviour
{
    [SerializeField] Button button = null;
    [SerializeField] TMP_Text text = null;

    public string Text { set { text.text = value; } get { return text.text; } }
    public bool Interactable { set { button.interactable = value; } get { return button.interactable; } }

    public DialogManager.ButtonType Type { private set; get; }
    public UnityAction OnPressed { private set; get; }
}