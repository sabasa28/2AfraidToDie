using System;
using UnityEngine;

public class Phone : Interactable
{
    [SerializeField] UIPhone uiPhone;
    public static event Action<bool> OnTryToUsePhone; //bool StartUsing
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

    public void OnStopUsingPhone()
    {
        OnTryToUsePhone(false);
        uiPhone.gameObject.SetActive(false);
    }
}
