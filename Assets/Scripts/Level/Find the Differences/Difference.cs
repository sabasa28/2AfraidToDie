using System;
using UnityEngine;

public class Difference : Interactable
{
    public static event Action<Difference> OnSelected;

    Vector3 posA;
    Vector3 rotA;
    [SerializeField] Vector3 posB;
    [SerializeField] Vector3 rotB;


    protected override void Awake()
    {
        base.Awake();

        posA = transform.localPosition;
        rotA = transform.localRotation.eulerAngles;
    }

    public void SetTransformAsDifference(bool isDifference)
    {
        if (isDifference)
        {
            transform.localPosition = posB;
            transform.localRotation = Quaternion.Euler(rotB);
        }
        else
        {
            transform.localPosition = posA;
            transform.localRotation = Quaternion.Euler(rotA);
        }
    }

    public override void OnClicked()
    {
        base.OnClicked();

        OnSelected?.Invoke(this);
    }
}