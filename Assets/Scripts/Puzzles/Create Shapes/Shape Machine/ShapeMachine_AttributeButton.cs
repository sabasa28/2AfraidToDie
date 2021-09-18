using UnityEngine;

public class ShapeMachine_AttributeButton : Interactable
{
    [SerializeField] bool increase = true;
    [SerializeField] ShapeMachine_AttributeScreen attributeScreen = null;

    public override void OnClicked()
    {
        base.OnClicked();

        attributeScreen.ChangeAttribute(increase);
    }
}