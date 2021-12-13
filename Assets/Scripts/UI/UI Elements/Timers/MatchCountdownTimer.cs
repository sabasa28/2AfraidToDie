using System;
using UnityEngine;

public class MatchCountdownTimer : MonoBehaviour
{
    Animator animator;

    static public event Action OnCountdownFinished;

    void Awake() => animator = GetComponent<Animator>();

    public void StartCountdown() => animator.SetTrigger("Start countdown");

    public void StopCountdown() => animator.SetTrigger("Stop countdown");

    public void FinishCountdown() => OnCountdownFinished?.Invoke();

    public void Tick() => AudioManager.Get().PlayUISFX(AudioManager.UISFXs.CountdownTick);
}