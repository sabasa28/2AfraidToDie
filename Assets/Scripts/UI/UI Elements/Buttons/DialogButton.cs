using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogButton : Clickable
{
    [SerializeField] TMP_Text text = null;
    Button button;

    public string Text { set { text.text = value; } get { return text.text; } }
    public bool Interactable { set { button.interactable = value; } get { return button.interactable; } }

    public DialogManager.ButtonType Type { private set; get; }
    public UnityAction OnPressed { private set; get; }

    protected override void Awake()
    {
        base.Awake();

        button = GetComponent<Button>();
    }

    public void Press() => button.onClick.Invoke();
}