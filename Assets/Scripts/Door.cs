using UnityEngine;

public class Door : MonoBehaviour
{
    public void Open()
    {
        Debug.Log("door opened");
        gameObject.SetActive(false);
    }
}