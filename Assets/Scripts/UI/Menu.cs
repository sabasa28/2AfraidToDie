using UnityEngine;

public class Menu : MonoBehaviour
{
    [SerializeField] bool displayMargins = true;
    [Space]
    [SerializeField] Menu parentMenu = null;
    [SerializeField] Menu previousMenu = null;

    public bool DisplayMargins { get { return displayMargins; } }
    public Menu ParentMenu { get { return parentMenu; } }
    public Menu PreviousMenu { get { return previousMenu; } }
}