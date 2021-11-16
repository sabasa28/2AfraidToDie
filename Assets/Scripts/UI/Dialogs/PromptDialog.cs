using TMPro;
using UnityEngine;

public class PromptDialog : Dialog
{
    [Header("Prompt Dialog")]
    [SerializeField] TMP_InputField inputField = null;

    public TMP_InputField InputField { get { return inputField; } }

    void Start() => inputField.Select();

    public void CheckInputText(string text) => Buttons[DialogManager.ButtonType.Continue].Interactable = !string.IsNullOrEmpty(text);
}