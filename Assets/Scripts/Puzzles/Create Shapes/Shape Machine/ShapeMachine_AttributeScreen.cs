using System;
using UnityEngine;

public class ShapeMachine_AttributeScreen /*<- nombre placeholder*/ : MonoBehaviour
{
    public string currentAttribute;
    [SerializeField] string[] attributes = null;

    int currentAttributeIndex = 0;

    public Action<ShapeMachine_AttributeScreen> OnAttributeChanged;

    void Awake()
    {
        if (attributes.Length > 0) currentAttribute = attributes[currentAttributeIndex];
    }

    public void ChangeAttribute(bool increase)
    {
        if (attributes.Length == 0) return;

        if (increase)
        {
            currentAttributeIndex++;

            if (currentAttributeIndex >= attributes.Length) currentAttributeIndex = 0;
        }
        else
        {
            currentAttributeIndex--;

            if (currentAttributeIndex < 0) currentAttributeIndex = attributes.Length -1;
        }
        currentAttribute = attributes[currentAttributeIndex];

        OnAttributeChanged?.Invoke(this);
    }
}