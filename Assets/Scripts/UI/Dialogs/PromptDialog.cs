using TMPro;
using UnityEngine;

public class PromptDialog : Dialog
{
    [SerializeField] TMP_InputField inputField = null;

    public TMP_InputField InputField { get { return inputField; } }
}