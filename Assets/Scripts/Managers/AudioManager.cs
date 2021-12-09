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

    public bool SoundOn { set { soundOn = value; } get { return soundOn; } }
    public bool MusicOn { set { musicOn = value; } get { return musicOn; } }

    //void OnEnable() => SceneManager.sceneLoaded += PlayMusicOnNewScene;

    //void OnDisable() => SceneManager.sceneLoaded -= PlayMusicOnNewScene;

    void PlayMusicOnNewScene(Scene scene, LoadSceneMode mode)
    {
        Songs song;

        switch (scene.name)
        {
            case "Main Menu":
                song = Songs.MainMenu;
                break;
            case "Gameplay":
                song = Songs.Gameplay;
                break;
            default:
                song = 0;
                break;
        }

        PlayMusic(song);
    }

    public void PlaySFX(AudioClip sfx) { if (soundOn) AudioSource.PlayClipAtPoint(sfx, Vector3.zero); }

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