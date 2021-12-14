using System.Collections;
using UnityEngine;

public class Host : MonoBehaviour
{
    [SerializeField] DialogueCharacterSO dialogueCharacter = null;

    [Header("Dialogues")]
    [SerializeField] float timeLeftForTimeDialogue = 60.0f;
    [SerializeField] float buttonDialogueInterval = 20.0f;
    bool timeDialoguePlayed = false;
    bool checkForButtonDialogue = false;

    [Header("Movement")]
    [SerializeField] float movementSpeed;
    [SerializeField] float[] stopPoints;
    [SerializeField] Animator GfxAnimator;
    int currentStopPoint = 0;

    [Header("Cinematic")]
    [SerializeField] Vector3 positionInCinematic;
    [SerializeField] Transform cameraPivot;
    [SerializeField] GameObject cinematicCamera;

    void Awake() => dialogueCharacter.Initialize();

    void OnEnable()
    {
        GameplayController.OnTimerUpdated += CheckTimerForTimeDialogue;
        GameplayController.OnPlayerMistake += () => dialogueCharacter.PlayRandomDialogue("Mistake");
        GameplayController.OnPlayerGuess += () => dialogueCharacter.PlayRandomDialogue("Guess");
        GameplayController.OnAllDifferencesSelected += () => dialogueCharacter.PlayRandomDialogue("Guess");
        GameplayController.OnAllDifferencesSelected += StartCheckingForButtonDialogue;
        GameplayController.OnRoomCompleted += AdvanceToNextStop;

        Door.OnExitDoorUnlocked += StopCheckingForButtonDialogue;
    }

    void Start() => dialogueCharacter.PlayDialogue("Introduction", 0);

    void OnDisable()
    {
        GameplayController.OnTimerUpdated -= CheckTimerForTimeDialogue;
        GameplayController.OnPlayerMistake -= () => dialogueCharacter.PlayRandomDialogue("Mistake");
        GameplayController.OnPlayerGuess -= () => dialogueCharacter.PlayRandomDialogue("Guess");
        GameplayController.OnAllDifferencesSelected -= () => dialogueCharacter.PlayRandomDialogue("Guess");
        GameplayController.OnAllDifferencesSelected -= StartCheckingForButtonDialogue;
        GameplayController.OnRoomCompleted -= AdvanceToNextStop;

        Door.OnExitDoorUnlocked += StopCheckingForButtonDialogue;
    }


    #region Dialogue

    void CheckTimerForTimeDialogue(float newTime)
    {
        if (!timeDialoguePlayed && newTime <= timeLeftForTimeDialogue)
        {
            dialogueCharacter.PlayRandomDialogue("Running out of time");
            timeDialoguePlayed = true;
        }
    }

    void StartCheckingForButtonDialogue()
    {
        checkForButtonDialogue = true;
        StartCoroutine(CheckForButtonDialogue());
    }

    void StopCheckingForButtonDialogue() => checkForButtonDialogue = false;

    IEnumerator CheckForButtonDialogue()
    {
        float timer = buttonDialogueInterval;

        while (checkForButtonDialogue)
        {
            timer -= Time.deltaTime;

            if (timer <= 0.0f)
            {
                dialogueCharacter.PlayRandomDialogue("Button press");
                timer = buttonDialogueInterval;
            }

            yield return null;
        }
    }
    #endregion

    #region Movement

    public void AdvanceToNextStop(int door)
    {
        Vector3 posToGoTo;
        
        if (door <= currentStopPoint) return;
        currentStopPoint++;

        if (currentStopPoint > stopPoints.Length - 1)
        {
            posToGoTo = positionInCinematic;
        }
        else posToGoTo = new Vector3(stopPoints[currentStopPoint], transform.position.y, transform.position.z);
        
        StartCoroutine(AdvanceAndStop(posToGoTo));
    }

    IEnumerator AdvanceAndStop(Vector3 posToAdvanceTo)
    {
        GfxAnimator.SetBool("Advance", true);
        yield return new WaitForSeconds(0.5f);
        Vector3 startingPos = transform.position;
        float t = 0;
        while (transform.position != posToAdvanceTo)
        {
            transform.position = Vector3.Lerp(startingPos, posToAdvanceTo, t);
            t += Time.deltaTime * movementSpeed;
            if (t > 1.0f) t = 1.0f;
            yield return null;
        }
        GfxAnimator.SetBool("Advance", false);
        if (currentStopPoint > stopPoints.Length - 1)
        {
            GfxAnimator.SetBool("Cinematic",true);
            yield return new WaitForSeconds(2);
            dialogueCharacter.PlayRandomDialogue("Victory");
            Camera.main.gameObject.SetActive(false);
            cinematicCamera.SetActive(true);
            yield return new WaitForSeconds(8);
            GameplayController.Get().ProcessLevelEnd();
        }
    }

    #endregion
}