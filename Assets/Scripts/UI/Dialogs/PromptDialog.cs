using TMPro;
using UnityEngine;

public class PromptDialog : Dialog
{
    [SerializeField] TMP_Text titleText = null;
    [SerializeField] TMP_InputField inputField = null;

    public string Title { set { titleText.text = value; } get { return titleText.text; } }
    public TMP_InputField InputField { get { return inputField; } }
}