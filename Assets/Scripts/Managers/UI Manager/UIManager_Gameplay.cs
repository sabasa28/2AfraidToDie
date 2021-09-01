using UnityEngine;
using UnityEngine.UI;

public class UIManager_Gameplay : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] Text timerText = null;

    void OnEnable()
    {
        GameplayController.OnTimerUpdated += UpdateTimerText;
    }

    void UpdateTimerText(float newTime)
    {
        if (newTime < 0.0f) newTime = 0.0f;
        timerText.text = "Time: " + newTime;
    }

    void OnDisable()
    {
        GameplayController.OnTimerUpdated -= UpdateTimerText;
    }
}