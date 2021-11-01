using System;
using UnityEngine;

public class TitleScreen : MonoBehaviour
{
    static public event Action OnTitleScreenClosed;

    void Update() { if (Input.anyKeyDown) OnTitleScreenClosed?.Invoke(); }
}