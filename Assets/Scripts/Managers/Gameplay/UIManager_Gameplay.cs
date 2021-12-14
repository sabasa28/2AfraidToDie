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

    NetworkManager networkManager;

    static public event Action<bool> OnPauseToggled;

    void OnEnable()
    {
        AudioManager.OnDialoguePlayed += DisplayDialogueSubtitle;
        AudioManager.OnDialogueStopped += EraseDialogueSubtitle;

        GameplayController.OnTimerUpdated += UpdateTimerText;
        GameplayController.OnPlayerMistake += NegativeFeedback;
        GameplayController.OnLevelEnd += DisplayVictoryScreen;

        Door.OnDoorClosed += HideControlsScreen;
    }

    void Awake()
    {
        timeText.text = "";
        dialogueText.text = "";

        networkManager = NetworkManager.Get();
        returnToMainMenuButton.onClick.AddListener(networkManager.LeaveRoom);
        victoryMainMenuButton.onClick.AddListener(networkManager.LeaveRoom);

        differenceCount = GameplayController.Get().DifferenceCount;
    }
    
    void Update()
    {
        if (canPause && Input.GetButtonUp("Pause"))
        {
            SetPauseMenuActive(!pauseMenu.activeInHierarchy);
            AudioManager.Get().PlayUISFX(AudioManager.UISFXs.Click);
        }
    }

    void OnDisable()
    {
        AudioManager.OnDialoguePlayed -= DisplayDialogueSubtitle;
        AudioManager.OnDialogueStopped -= EraseDialogueSubtitle;

        GameplayController.OnTimerUpdated -= UpdateTimerText;
        GameplayController.OnPlayerMistake -= NegativeFeedback;
        GameplayController.OnLevelEnd -= DisplayVictoryScreen;

        Door.OnDoorClosed -= HideControlsScreen;
    }

    #region Timer
    void UpdateTimerText(float newTime)
    {
        if (newTime <= 0.0f)
        {
            newTime = 0.0f;
            NegativeFeedback();
        }
        timeText.text = ToMinutes(newTime);

        timeSlider.value = newTime / GameplayController.Get().TimerDuration;
    }

    string ToMinutes(float time)
    {
        int iTime = (int)time;
        int minutes = iTime / 60;
        int seconds = iTime % 60;

        return minutes + ":" + seconds.ToString("00");
    }
    #endregion

    #region Timer
    void NegativeFeedback()
    {
        puzzleInfoAnimator.SetTrigger("OnFail");
        timerAnimator.SetTrigger("OnFeedback");
    }
    #endregion

    #region Dialogue
    void DisplayDialogueSubtitle(DialogueCharacterSO.Dialogue dialogue)
    {
        dialogueText.text = dialogue.subtitle;
        dialogueText.gameObject.SetActive(true);
    }

    void EraseDialogueSubtitle()
    {
        dialogueText.gameObject.SetActive(false);
        dialogueText.text = "";
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
        GameManager.Get().ControlMode = state ? GameManager.ControlModes.UI : GameManager.ControlModes.Gameplay;

        OnPauseToggled?.Invoke(state);
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