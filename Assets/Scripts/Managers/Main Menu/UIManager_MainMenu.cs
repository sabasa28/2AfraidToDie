using System;
using UnityEngine;

public class UIManager_MainMenu : MonoBehaviour
{
    static public event Action OnExit;
    static public event Action<bool> OnPlay;

    public void Exit()
    {
        OnExit?.Invoke();
    }

    public void Play(bool playAsPA)
    {
        OnPlay?.Invoke(playAsPA);
    }
}