using System;
using UnityEngine;

public class AudioManager : PersistentMonoBehaviourSingleton<AudioManager>
{
    public enum SFXAudioSources
    {
        UI,
        SFX,
        Dialogue
    }

    public enum UISFXs
    {
        Click,
        CountdownTick,
    }

    public enum GameplaySFXs
    {
        Guess,
        Mistake
    }

    public enum Songs
    {
        Pre_Gameplay,
        Gameplay
    }

    [Header("Audio Sources")]
    [SerializeField] AudioSource[] sfxAudioSources;
    [SerializeField] AudioSource musicAudioSource;

    [Header("Audio Clips")]
    [SerializeField] AudioClip[] uiSFXs;
    [SerializeField] AudioClip[] gameplaySFXs;
    [SerializeField] AudioClip[] music;

    [Header("Sound Options")]
    [SerializeField] bool soundOn = true;
    [SerializeField] bool musicOn = true;
    [Space]
    [SerializeField, Range(0.0f, 1.0f)] float soundBaseVolume = 1.0f;
    [SerializeField, Range(0.0f, 1.0f)] float musicBaseVolume = 1.0f;

    bool playingDialogue = false;
    GameManager gameManager;

    public bool SoundOn { set { soundOn = value; } get { return soundOn; } }
    public bool MusicOn { set { musicOn = value; } get { return musicOn; } }
    public Songs CurrentSong { private set; get; }

    static public event Action<DialogueCharacterSO.Dialogue> OnDialoguePlayed;
    static public event Action OnDialogueStopped;

    public override void Awake()
    {
        base.Awake();

        gameManager = GameManager.Get();
    }

    void OnEnable()
    {
        GameManager.OnSceneLoaded += StopSFXOnNewScene;
        GameManager.OnSceneLoaded += PlayMusicOnNewScene;

        AudioController_Gameplay.OnSoundVolumeUpdate += UpdateSoundVolume;
        AudioController_Gameplay.OnMusicVolumeUpdate += UpdateMusicVolume;
    }

    void Start()
    {
        musicAudioSource.volume = musicBaseVolume;

        foreach (AudioSource source in sfxAudioSources) source.volume = soundBaseVolume;
    }

    void Update()
    {
        if (playingDialogue && !sfxAudioSources[(int)SFXAudioSources.Dialogue].isPlaying)
        {
            playingDialogue = false;
            OnDialogueStopped?.Invoke();
        }
    }

    void OnDisable()
    {
        GameManager.OnSceneLoaded -= StopSFXOnNewScene;
        GameManager.OnSceneLoaded -= PlayMusicOnNewScene;

        AudioController_Gameplay.OnSoundVolumeUpdate -= UpdateSoundVolume;
        AudioController_Gameplay.OnMusicVolumeUpdate -= UpdateMusicVolume;
    }

    void UpdateSoundVolume(float volume) { foreach (AudioSource source in sfxAudioSources) source.volume = volume * soundBaseVolume; }

    void UpdateMusicVolume(float volume) => musicAudioSource.volume = volume * musicBaseVolume;

    void StopSFXOnNewScene(string sceneName)
    {
        foreach (AudioSource source in sfxAudioSources)
        {
            do source.Stop();
            while (source.isPlaying);
        }
    }

    void PlayMusicOnNewScene(string sceneName)
    {
        bool playNewSong = false;
        Songs song = 0;

        if (sceneName == gameManager.MainMenuScene)
        {
            playNewSong = true;
            song = Songs.Pre_Gameplay;
        }

        if (playNewSong) PlayMusic(song);
    }

    public void PlayUISFX(UISFXs sfx)
    {
        if (!soundOn) return;

        AudioSource source = sfxAudioSources[(int)SFXAudioSources.UI];

        source.clip = uiSFXs[(int)sfx];
        source.Play();
    }

    public void PlayGameplaySFX(GameplaySFXs sfx)
    {
        if (!soundOn) return;

        AudioSource source = sfxAudioSources[(int)SFXAudioSources.SFX];

        source.clip = gameplaySFXs[(int)sfx];
        source.Play();
    }

    public void PlayDialogue(DialogueCharacterSO.Dialogue dialogue)
    {
        if (!soundOn) return;

        AudioSource source = sfxAudioSources[(int)SFXAudioSources.Dialogue];

        do source.Stop();
        while (source.isPlaying);
        OnDialogueStopped?.Invoke();

        source.clip = dialogue.audioClip;
        source.Play();
        playingDialogue = true;
        OnDialoguePlayed?.Invoke(dialogue);
    }

    public void PlayMusic(Songs song)
    {
        musicAudioSource.clip = music[(int)song];
        CurrentSong = song;

        if (musicOn) musicAudioSource.Play();
    }

    public void ToggleSound() => soundOn = !soundOn;

    public void ToggleMusic()
    {
        musicOn = !musicOn;

        if (musicOn) musicAudioSource.Play();
        else musicAudioSource.Stop();
    }
}