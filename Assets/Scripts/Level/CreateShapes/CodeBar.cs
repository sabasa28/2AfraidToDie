using System;
using UnityEngine;
using TMPro;

public class CodeBar : Grabbable
{
    public DeliveryMachine deliveryMachine;
    int code;
    public Animator animator;
    [SerializeField] TextMeshProUGUI codeText;

    public static Action UpdatePuzzleProgress;

    public override void OnClicked()
    {
        base.OnClicked();
        UpdatePuzzleProgress();
    }
    public void SetCode(int newCode)
    {
        code = newCode;
        codeText.text = code.ToString();
    }

    public int GetCode()
    {
        return code;
    }
}
