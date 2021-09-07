using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager_Gameplay : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] TextMeshProUGUI timerText = null;

    [Header("Dialogue")]
    [SerializeField] TextMeshProUGUI dialogueText = null;
    [SerializeField] Animator timerAnim = null;
    [SerializeField] float dialogueDuration = 5.0f;

    [Header("Puzzle Info")]
    [SerializeField] Animator puzzleInfoAnim = null;
    [SerializeField] TextMeshProUGUI puzzleInfoText = null;
    public string puzzleVariableName;

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
    void UpdateTimerText(float newTime, bool playNegativeFeedback)
    {
        if (newTime < 0.0f) newTime = 0.0f;
        timerText.text = "Time: " + newTime.ToString("0.0");
        if (playNegativeFeedback) timerAnim.SetTrigger("NegativeFeedback");
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

    #region Puzzle Info
    public void UpdatePuzzleInfoText(int newNum, bool positiveFeedback)
    {
        puzzleInfoText.text = puzzleVariableName + " " + newNum;
        if (positiveFeedback) puzzleInfoAnim.SetTrigger("PositiveFeedback");
    }
    public void PuzzleInfoTextActiveState(bool newState)
    {
        puzzleInfoText.gameObject.SetActive(newState);
    }
    #endregion
    IEnumerator EraseTextWithTimer(TextMeshProUGUI text, float time)
    {
        yield return new WaitForSeconds(time);
        text.text = "";
    }
}