using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue Character", menuName = "Dialogue Character")]
public class DialogueCharacterSO : ScriptableObject
{
    [Serializable]
    public struct Dialogue
    {
        public AudioClip audioClip;
        [TextArea] public string subtitle;
    }

    [Serializable]
    public struct DialogueList
    {
        public string name;

        public Dialogue[] dialogues;
        [HideInInspector] public AudioClip lastRandomClipPlayed;
    }

    AudioManager audioManager;

    [SerializeField] DialogueList[] dialogueLists;

    public Dictionary<string, DialogueList> DialogueListsByName { private set; get; }

    public void Initialize()
    {
        audioManager = AudioManager.Get();

        DialogueListsByName = new Dictionary<string, DialogueList>();
        foreach (DialogueList list in dialogueLists) DialogueListsByName.Add(list.name, list);
    }

    public void PlayDialogue(string listName, int index)
    {
        if (!DialogueListsByName.ContainsKey(listName))
        {
            Debug.LogError("Can not play dialogue: the given list does not exist");
            return;
        }
        else if (index < 0 || index >= DialogueListsByName[listName].dialogues.Length)
        {
            Debug.LogError("Can not play dialogue: index is out of range");
            return;
        }

        audioManager.PlayDialogue(DialogueListsByName[listName].dialogues[index]);
    }

    public void PlayRandomDialogue(string listName)
    {
        if (!DialogueListsByName.ContainsKey(listName))
        {
            Debug.LogError("Can not play dialogue: the given list does not exist");
            return;
        }

        int index = 0;
        DialogueList list = DialogueListsByName[listName];

        if (list.dialogues.Length > 1)
        {
            if (list.lastRandomClipPlayed)
            {
                do index = UnityEngine.Random.Range(0, list.dialogues.Length);
                while (list.dialogues[index].audioClip == list.lastRandomClipPlayed);
            }
            else index = UnityEngine.Random.Range(0, list.dialogues.Length);
        }

        Dialogue dialogue = list.dialogues[index];
        list.lastRandomClipPlayed = dialogue.audioClip;
        DialogueListsByName[listName] = list;

        audioManager.PlayDialogue(dialogue);
    }
}