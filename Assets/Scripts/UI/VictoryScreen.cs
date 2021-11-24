using UnityEngine;

public class VictoryScreen : MonoBehaviour
{
    [SerializeField] GameObject container = null;
    [SerializeField] GameObject mainMenuButton = null;
    [SerializeField] GameObject waitText = null;

    public void Display(bool isMaster)
    {
        if (isMaster) mainMenuButton.SetActive(true);
        else waitText.SetActive(true);

        container.SetActive(true);
    }
}