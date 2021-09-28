using System;

public class Difference : Interactable
{
    public static event Action<Difference> OnSelected;

    public override void OnClicked()
    {
        base.OnClicked();

        OnSelected?.Invoke(this);
    }
}