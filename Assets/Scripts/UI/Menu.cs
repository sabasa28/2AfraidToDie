using UnityEngine;

public class Menu : MonoBehaviour
{
    [SerializeField] bool displayMargins = true;
    [SerializeField] Menu previousMenu = null;

    public bool DisplayMargins { get { return displayMargins; } }
    public Menu PreviousMenu { get { return previousMenu; } }
}