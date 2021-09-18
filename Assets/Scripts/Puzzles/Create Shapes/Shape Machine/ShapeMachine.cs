using System.Collections.Generic;
using UnityEngine;

public class ShapeMachine : MonoBehaviour
{
    public enum AttributeTypes
    {
        Color,
        Symbol,
        Shape
    }

    [SerializeField] ShapeMachine_AttributeScreen[] attributeScreens = null;

    List<string> attributes = null;

    void Awake()
    {
        attributes = new List<string>();

        foreach (ShapeMachine_AttributeScreen screen in attributeScreens)
        {
            attributes.Add(screen.currentAttribute);
            screen.OnAttributeChanged = UpdateAttributes;
        }
    }

    void UpdateAttributes(ShapeMachine_AttributeScreen screen)
    {
        for (int i = 0; i < attributeScreens.Length; i++)
            if (screen == attributeScreens[i]) attributes[i] = screen.currentAttribute;

        string shape = "Shape: ";
        foreach (string shapeAttribute in attributes) shape += shapeAttribute + ", ";
        Debug.Log(shape);
    }
}