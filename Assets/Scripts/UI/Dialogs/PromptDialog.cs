using TMPro;
using UnityEngine;

public class PromptDialog : Dialog
{
    [Header("Prompt Dialog")]
    [SerializeField] TMP_InputField inputField = null;

    public TMP_InputField InputField { get { return inputField; } }

    void Start()
    {
        inputField.Select();
        CheckInputText();
    }

    void Update() { if (Input.GetButtonDown("Submit") && CanSubmit()) Buttons[DialogManager.ButtonType.Continue].Press(); }

    bool CanSubmit() => !string.IsNullOrEmpty(inputField.text);

    public void CheckInputText() => Buttons[DialogManager.ButtonType.Continue].Interactable = CanSubmit();
}