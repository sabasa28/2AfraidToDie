using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager_Gameplay : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] TextMeshProUGUI timeText = null;
    [SerializeField] Slider timeSlider = null;
    [SerializeField] Animator timerAnimator = null;

    [Header("Dialogue")]
    [SerializeField] TextMeshProUGUI dialogueText = null;
    [SerializeField] float dialogueDuration = 5.0f;

    [Header("Puzzle Info")]
    [SerializeField] Animator puzzleInfoAnimator = null;
    [SerializeField] Animator differenceCounterAnimator = null;
    [SerializeField] TextMeshProUGUI puzzleInfoText = null;
    public string puzzleVariableName;
    int differenceCount;

    [Header("Menues")]
    [SerializeField] GameObject pauseMenu = null;
    [SerializeField] GameObject victoryScreen = null;

    public static event Action<bool> OnPauseMenuStateSwitched;
    public static event Action OnGoToMainMenu;

    void OnEnable()
    {
        GameplayController.OnTimerUpdated += UpdateTimerText;
        GameplayController.OnLevelEnd += DisplayVictoryScreen;

        DialogueManager.OnDialogueLinePlayed += UpdateDialogueText;
    }

    void Awake()
    {
        timeText.text = "";
        dialogueText.text = "";

        differenceCount = GameplayController.Get().DifferenceCount;
    }

    void Update() { if (Input.GetButtonUp("Pause")) SetPauseMenuActive(!pauseMenu.activeInHierarchy); }

    void OnDisable()
    {
        GameplayController.OnTimerUpdated -= UpdateTimerText;
        GameplayController.OnLevelEnd -= DisplayVictoryScreen;

        DialogueManager.OnDialogueLinePlayed -= UpdateDialogueText;
    }

    #region Timer
    void UpdateTimerText(float newTime, bool playNegativeFeedback)
    {
        if (newTime < 0.0f) newTime = 0.0f;
        timeText.text = ToMinutes(newTime);

        timeSlider.value = newTime / GameplayController.Get().TimerDuration;

        if (playNegativeFeedback)
        {
            puzzleInfoAnimator.SetTrigger("OnFail");
            timerAnimator.SetTrigger("OnFeedback");
        }
    }

    string ToMinutes(float time)
    {
        int iTime = (int)time;
        int minutes = iTime / 60;
        int seconds = iTime % 60;

        return minutes + ":" + seconds.ToString("00");
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
    public void UpdatePuzzleInfoText(int newNumber, bool positiveFeedback)
    {
        puzzleInfoText.text = newNumber + "/" + differenceCount;
        if (positiveFeedback) differenceCounterAnimator.SetTrigger("OnFeedback");
    }

    public void PuzzleInfoTextActiveState(bool newState) => puzzleInfoText.gameObject.SetActive(newState);
    #endregion

    #region Victory Screen
    void DisplayVictoryScreen() => victoryScreen.SetActive(true);
    #endregion

    #region ButtonFunctions
    public void SetPauseMenuActive(bool state)
    {
        pauseMenu.SetActive(state);

        OnPauseMenuStateSwitched?.Invoke(state);
    }

    public void GoToMainMenu() => OnGoToMainMenu?.Invoke();
    #endregion

    #region Coroutines
    IEnumerator EraseTextWithTimer(TextMeshProUGUI text, float time)
    {
        yield return new WaitForSeconds(time);
        text.text = "";
    }
    #endregion
}