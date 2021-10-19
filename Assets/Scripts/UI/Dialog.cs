using TMPro;
using UnityEngine;

public class Dialog : Menu
{
    [Space]
    [SerializeField] string message = "Message";
    [SerializeField] TMP_Text messageText = null;

    public string Message
    {
        set
        {
            message = value;
            messageText.text = message;
        }
        get { return message; }
    }

    void Start() => messageText.text = message;
}