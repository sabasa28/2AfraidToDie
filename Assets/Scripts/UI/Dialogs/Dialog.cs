using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Dialog : MonoBehaviour
{
    [SerializeField] TMP_Text messageText = null;
    [SerializeField] GameObject buttonPrefab = null;
    [SerializeField] Transform buttonContainer = null;

    public string Message { set { messageText.text = value; } get { return messageText.text; } }
    public Dictionary<DialogManager.ButtonType, DialogButton> Buttons { private set; get; } = new Dictionary<DialogManager.ButtonType, DialogButton>();

    public void AddButton(string text, UnityAction action, DialogManager.ButtonType type)
    {
        GameObject newButtonGO = Instantiate(buttonPrefab, buttonContainer);

        newButtonGO.GetComponent<Button>().onClick.AddListener(action);

        DialogButton newButton = newButtonGO.GetComponent<DialogButton>();
        newButton.Text = text;
        Buttons.Add(type, newButton);
    }
}