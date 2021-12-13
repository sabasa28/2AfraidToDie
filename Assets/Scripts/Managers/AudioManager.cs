using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : PersistentMonoBehaviourSingleton<AudioManager>
{
    public enum Sounds
    {
        Button,
        PlayerShot,
        EnemyShot,
        Item,
        Explosion
    }

    public enum Songs
    {
        MainMenu,
        Gameplay
    }

    [Header("Audio Sources")]
    [SerializeField] AudioSource sfxAudioSource;
    [SerializeField] AudioSource musicAudioSource;

    [Header("Audio Clips")]
    [SerializeField] AudioClip[] music;

    [Header("Sound Options")]
    [SerializeField] bool soundOn = true;
    [SerializeField] bool musicOn = true;

    GameManager gameManager;

    public bool SoundOn { set { soundOn = value; } get { return soundOn; } }
    public bool MusicOn { set { musicOn = value; } get { return musicOn; } }

    public override void Awake()
    {
        base.Awake();

        gameManager = GameManager.Get();
    }

    void OnEnable()
    {
        GameManager.OnSceneLoaded -= StopSFXOnNewScene;
        //SceneManager.sceneLoaded += PlayMusicOnNewScene;
    }

    void OnDisable()
    {
        GameManager.OnSceneLoaded += StopSFXOnNewScene;
        //SceneManager.sceneLoaded -= PlayMusicOnNewScene;
    }

    void StopSFXOnNewScene(string sceneName) { if (sfxAudioSource.isPlaying) sfxAudioSource.Stop(); }

    void PlayMusicOnNewScene(string sceneName)
    {
        Songs song = 0;

        if (sceneName == gameManager.MainMenuScene) song = Songs.MainMenu;
        else if (sceneName == gameManager.GameplayScene) song = Songs.Gameplay;

        PlayMusic(song);
    }

    public void PlaySFX(AudioClip sfx)
    {
        if (soundOn)
        {
            if (sfxAudioSource.isPlaying) sfxAudioSource.Stop();

            sfxAudioSource.clip = sfx;
            sfxAudioSource.Play();
        }
    }

    public void PlayMusic(Songs song)
    {
        musicAudioSource.clip = music[(int)song];

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