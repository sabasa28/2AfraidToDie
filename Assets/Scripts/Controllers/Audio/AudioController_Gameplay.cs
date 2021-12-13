using System;
using UnityEngine;
using UnityEngine.UI;

public class AudioController_Gameplay : MonoBehaviour
{
    [SerializeField] Slider soundVolumeSlider = null;
    [SerializeField] Slider musicVolumeSlider = null;

    AudioManager audioManager;

    static public event Action<float> OnSoundVolumeUpdate;
    static public event Action<float> OnMusicVolumeUpdate;

    void Awake() => audioManager = AudioManager.Get();

    void OnEnable()
    {
        soundVolumeSlider.onValueChanged.AddListener(UpdateSoundVolume);
        musicVolumeSlider.onValueChanged.AddListener(UpdateMusicVolume);

        GameplayController.OnPlayerGuess += () => audioManager.PlayGameplaySFX(AudioManager.GameplaySFXs.Guess);
        GameplayController.OnPlayerMistake += () => audioManager.PlayGameplaySFX(AudioManager.GameplaySFXs.Mistake);

        Door.OnEntranceDoorOpen += CheckForGameplaySong;
    }

    void OnDisable()
    {
        soundVolumeSlider.onValueChanged.RemoveAllListeners();

        GameplayController.OnPlayerGuess -= () => audioManager.PlayGameplaySFX(AudioManager.GameplaySFXs.Guess);
        GameplayController.OnPlayerMistake -= () => audioManager.PlayGameplaySFX(AudioManager.GameplaySFXs.Mistake);

        Door.OnEntranceDoorOpen -= CheckForGameplaySong;
    }

    void UpdateSoundVolume(float volume) => OnSoundVolumeUpdate?.Invoke(volume);

    void UpdateMusicVolume(float volume) => OnMusicVolumeUpdate?.Invoke(volume);

    void CheckForGameplaySong() { if (audioManager.CurrentSong != AudioManager.Songs.Gameplay) audioManager.PlayMusic(AudioManager.Songs.Gameplay); }
}