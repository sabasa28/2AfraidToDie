using UnityEngine;

public class Menu : MonoBehaviour
{
    [SerializeField] Menu previousMenu = null;
    public Menu PreviousMenu { get { return previousMenu; } }
}