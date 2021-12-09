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
    }

    [SerializeField] DialogueList[] dialogueLists;
    public Dictionary<string, DialogueList> DialogueListsByName { private set; get; } = new Dictionary<string, DialogueList>();

    void Awake() { foreach (DialogueList list in dialogueLists) DialogueListsByName.Add(list.name, list); }
}