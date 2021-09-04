using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager_Gameplay : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] Text timerText = null;

    [Header("Dialogue")]
    [SerializeField] Text dialogueText = null;
    [SerializeField] float dialogueDuration = 5.0f;

    void OnEnable()
    {
        GameplayController.OnTimerUpdated += UpdateTimerText;
        DialogueManager.OnDialogueLinePlayed += UpdateDialogueText;
    }

    void Awake()
    {
        timerText.text = "";
        dialogueText.text = "";
    }

    void OnDisable()
    {
        GameplayController.OnTimerUpdated -= UpdateTimerText;
        DialogueManager.OnDialogueLinePlayed -= UpdateDialogueText;
    }

    #region Timer
    void UpdateTimerText(float newTime)
    {
        if (newTime < 0.0f) newTime = 0.0f;
        timerText.text = "Time: " + newTime.ToString("0.0");
    }
    #endregion

    #region Dialogue
    void UpdateDialogueText(DialogueManager.Characters character, string line)
    {
        string text = character.ToString() + ": " + line;
        dialogueText.text = text;

        StartCoroutine(EraseTextWithTimer(dialogueText, dialogueDuration));
    }
    #endregion

    IEnumerator EraseTextWithTimer(Text text, float time)
    {
        yield return new WaitForSeconds(time);
        text.text = "";
    }
}