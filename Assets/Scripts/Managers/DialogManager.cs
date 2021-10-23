﻿using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DialogManager : MonoBehaviourSingleton<DialogManager>
{
    public class ButtonData
    {
        public string Text { private set; get; }
        public ButtonType Type { private set; get; }
        public UnityAction OnPressed { private set; get; }

        public ButtonData(string _text, ButtonType _type, UnityAction _onPressed)
        {
            Text = _text;
            Type = _type;
            OnPressed = _onPressed;
        }
    }

    public enum ButtonType
    {
        Positive,
        Negative,
        Close,
        Continue,
        Cancel
    }

    [SerializeField] GameObject cover = null;
    [SerializeField] GameObject defaultDialogPrefab = null;
    [SerializeField] GameObject promptDialogPrefab = null;
    [SerializeField] Transform dialogContainer = null;

    [Header("Default Button Texts")]
    [SerializeField] string defaultPositive = "";
    [SerializeField] string defaultNegative = "";
    [SerializeField] string defaultClose = "";
    [SerializeField] string defaultContinue = "";
    [SerializeField] string defaultCancel = "";

    public bool CoverActive { get { return cover.activeInHierarchy; } }

    Dialog GenerateDialog(GameObject prefab, string message, ButtonData[] buttons)
    {
        cover.SetActive(true);

        Vector2 position = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Dialog newDialog = Instantiate(prefab, position, Quaternion.identity, dialogContainer).GetComponent<Dialog>();

        newDialog.Message = message;

        foreach (ButtonData button in buttons) AddButtonToDialog(button, newDialog);

        return newDialog;
    }

    void AddButtonToDialog(ButtonData button, Dialog dialog)
    {
        string text = "";
        if (!string.IsNullOrEmpty(button.Text)) text = button.Text;
        else
        {
            switch (button.Type)
            {
                case ButtonType.Positive:
                    text = defaultPositive;
                    break;
                case ButtonType.Negative:
                    text = defaultNegative;
                    break;
                case ButtonType.Close:
                    text = defaultClose;
                    break;
                case ButtonType.Continue:
                    text = defaultContinue;
                    break;
                case ButtonType.Cancel:
                    text = defaultCancel;
                    break;
                default:
                    break;
            }
        }
        
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
        UnityAction action = () => CloseDialog(dialog.gameObject, currentSelected);
        if (button.OnPressed != null) action += button.OnPressed;

        dialog.AddButton(text, action, button.Type);
    }

    void CloseDialog(GameObject menuObject, GameObject firstSelected)
    {
        cover.SetActive(false);
        EventSystem.current.SetSelectedGameObject(firstSelected);

        Destroy(menuObject);
    }

    public Dialog DisplayMessageDialog(string message, string close, UnityAction onClose)
    {
        ButtonData closeButton = new ButtonData(close, ButtonType.Close, onClose);

        return GenerateDialog(defaultDialogPrefab, message, new ButtonData[] { closeButton });
    }

    public Dialog DisplayConfirmDialog(string message, string positive, UnityAction onPositive, string negative, UnityAction onNegative)
    {
        ButtonData positiveButton = new ButtonData(positive, ButtonType.Positive, onPositive);
        ButtonData negativeButton = new ButtonData(negative, ButtonType.Negative, onNegative);

        return GenerateDialog(defaultDialogPrefab, message, new ButtonData[] { negativeButton, positiveButton });
    }

    public PromptDialog DisplayPromptDialog(string message, string @continue, UnityAction<string> onContinue, string cancel, UnityAction onCancel)
    {
        PromptDialog newDialog = null;

        ButtonData cancelButton = new ButtonData(cancel, ButtonType.Cancel, onCancel);
        ButtonData continueButton = new ButtonData(@continue, ButtonType.Continue, () => { onContinue(newDialog.InputField.text); });

        newDialog = GenerateDialog(promptDialogPrefab, message, new ButtonData[] { cancelButton, continueButton }) as PromptDialog;
        return newDialog;
    }
}