using System;
using UnityEngine;

public class Phone : Interactable
{
    [SerializeField] UIPhone uiPhone;
    public static event Action<bool> OnTryToUsePhone; //bool StartUsing, true=start false=stop
    public static event Action OnCorrectNumberInserted;
    public int correctNumber;
    public override void OnClicked()
    {
        base.OnClicked();
        OnStartUsingPhone();
    }

    void OnStartUsingPhone()
    {
        OnTryToUsePhone(true);
        uiPhone.gameObject.SetActive(true);
        uiPhone.phone = this;
    }

    public void OnStopUsingPhone(int numInserted)
    {
        OnTryToUsePhone(false);
        uiPhone.gameObject.SetActive(false);
        if (correctNumber == numInserted) OnCorrectNumberInserted();
    }
}
