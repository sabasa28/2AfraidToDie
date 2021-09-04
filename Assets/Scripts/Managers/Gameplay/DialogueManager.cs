using System;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public enum DialogueCategories
    {
        PuzzleIntructions,
        Victory
    }

    public enum Characters
    {
        None,
        Host
    }

    [Serializable]
    public struct DialogueCategory
    {
        public DialogueCategories category;

        public DialogueLine[] lines;
    }

    [Serializable]
    public struct DialogueLine
    {
        public string line;

        public Characters character;
    }

    public DialogueCategory[] categories;

    public static event Action<Characters, string> OnDialogueLinePlayed;

    public void PlayDialogueLine(DialogueLine line)
    {
        OnDialogueLinePlayed?.Invoke(line.character, line.line);
    }
}