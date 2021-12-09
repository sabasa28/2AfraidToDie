using UnityEngine;

public class AudioController_Gameplay : MonoBehaviour
{
    [Header("Dialogue Characters")]
    [SerializeField] DialogueCharacterSO host;

    AudioManager audioManager;

    void Awake() => audioManager = AudioManager.Get();

    void Start() => audioManager.PlaySFX(host.DialogueListsByName["Introduction"].audioClips[0]);
}