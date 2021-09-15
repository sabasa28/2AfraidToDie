using System;
using UnityEngine;

public class UIManager_MainMenu : MonoBehaviour
{
    static public event Action onExit;
    static public event Action<bool> onPlay;

    public void Exit()
    {
        onExit?.Invoke();
    }

    public void Play(bool playAsPA)
    {
        onPlay?.Invoke(playAsPA);
    }
}