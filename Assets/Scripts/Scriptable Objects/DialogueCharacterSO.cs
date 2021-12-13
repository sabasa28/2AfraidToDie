using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue Character", menuName = "Dialogue Character")]
public class DialogueCharacterSO : ScriptableObject
{
    [Serializable]
    public struct DialogueList
    {
        public string name;

        public AudioClip[] audioClips;
        [HideInInspector] public AudioClip lastRandomClipPlayed;

        public void SetLastRandomClipPlayed(AudioClip clip) => lastRandomClipPlayed = clip;
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
        else if (index < 0 || index >= DialogueListsByName[listName].audioClips.Length)
        {
            Debug.LogError("Can not play dialogue: index is out of range");
            return;
        }

        audioManager.PlayDialogue(DialogueListsByName[listName].audioClips[index]);
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

        if (list.audioClips.Length > 1)
        {
            if (list.lastRandomClipPlayed)
            {
                do index = UnityEngine.Random.Range(0, list.audioClips.Length);
                while (list.audioClips[index] == list.lastRandomClipPlayed);
            }
            else index = UnityEngine.Random.Range(0, list.audioClips.Length);
        }

        AudioClip clip = list.audioClips[index];
        DialogueListsByName[listName].SetLastRandomClipPlayed(clip);
        audioManager.PlayDialogue(clip);
    }
}