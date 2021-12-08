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
    [SerializeField] TextMeshProUGUI differenceCounterText = null;
    [SerializeField] Animator differenceCounterAnimator = null;
    [SerializeField] Animator puzzleInfoAnimator = null;
    int differenceCount;

    [Header("Menues")]
    [SerializeField] GameObject pauseMenu = null;
    [SerializeField] GameObject controlsScreen = null;
    [SerializeField] GameObject victoryScreen = null;
    bool canPause = true;

    [Header("Buttons")]
    [SerializeField] Button returnToMainMenuButton = null;
    [SerializeField] Button victoryMainMenuButton = null;

    public static event Action<bool> OnPauseMenuStateSwitched;

    void OnEnable()
    {
        GameplayController.OnTimerUpdated += UpdateTimerText;
        GameplayController.OnLevelEnd += DisplayVictoryScreen;

        DialogueManager.OnDialogueLinePlayed += UpdateDialogueText;

        Door.OnDoorClosed += HideControlsScreen;
    }

    void Awake()
    {
        timeText.text = "";
        dialogueText.text = "";

        returnToMainMenuButton.onClick.AddListener(NetworkManager.Get().LeaveRoom);
        victoryMainMenuButton.onClick.AddListener(NetworkManager.Get().LeaveRoom);

        differenceCount = GameplayController.Get().DifferenceCount;
    }

    void Update() { if (canPause && Input.GetButtonUp("Pause")) SetPauseMenuActive(!pauseMenu.activeInHierarchy); }

    void OnDisable()
    {
        GameplayController.OnTimerUpdated -= UpdateTimerText;
        GameplayController.OnLevelEnd -= DisplayVictoryScreen;

        DialogueManager.OnDialogueLinePlayed -= UpdateDialogueText;

        Door.OnDoorClosed -= HideControlsScreen;
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
        differenceCounterText.text = newNumber + "/" + differenceCount;
        if (positiveFeedback) differenceCounterAnimator.SetTrigger("OnFeedback");
    }
    #endregion

    #region Screens
    void HideControlsScreen() => controlsScreen.SetActive(false);

    void DisplayVictoryScreen()
    {
        canPause = false;
        victoryScreen.SetActive(true);
    }
    #endregion

    #region Button Functions
    public void SetPauseMenuActive(bool state)
    {
        pauseMenu.SetActive(state);

        OnPauseMenuStateSwitched?.Invoke(state);
    }
    #endregion

    #region Coroutines
    IEnumerator EraseTextWithTimer(TextMeshProUGUI text, float time)
    {
        yield return new WaitForSeconds(time);
        text.text = "";
    }
    #endregion
}