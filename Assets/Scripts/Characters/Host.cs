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

    void Awake() => dialogueCharacter.Initialize();

    void OnEnable()
    {
        GameplayController.OnTimerUpdated += CheckTimerForTimeDialogue;
        GameplayController.OnPlayerMistake += () => dialogueCharacter.PlayRandomDialogue("Mistake");
        GameplayController.OnPlayerGuess += () => dialogueCharacter.PlayRandomDialogue("Guess");
        GameplayController.OnAllDifferencesSelected += () => dialogueCharacter.PlayRandomDialogue("Victory");
        GameplayController.OnAllDifferencesSelected += StartCheckingForButtonDialogue;

        Door.OnExitDoorUnlocked += StopCheckingForButtonDialogue;
    }

    void Start() => dialogueCharacter.PlayDialogue("Introduction", 0);

    void OnDisable()
    {
        GameplayController.OnTimerUpdated -= CheckTimerForTimeDialogue;
        GameplayController.OnPlayerMistake -= () => dialogueCharacter.PlayRandomDialogue("Mistake");
        GameplayController.OnPlayerGuess -= () => dialogueCharacter.PlayRandomDialogue("Guess");
        GameplayController.OnAllDifferencesSelected -= () => dialogueCharacter.PlayRandomDialogue("Victory");
        GameplayController.OnAllDifferencesSelected -= StartCheckingForButtonDialogue;

        Door.OnExitDoorUnlocked += StopCheckingForButtonDialogue;
    }

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
}