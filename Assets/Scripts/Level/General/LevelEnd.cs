using System;
using UnityEngine;

public class LevelEnd : MonoBehaviour
{
    public static event Action OnLevelEndReached;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        OnLevelEndReached?.Invoke();
    }
}