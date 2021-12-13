using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : PersistentMonoBehaviourSingleton<GameManager>
{
    public enum ControlModes
    {
        UI,
        Gameplay
    }

    [Header("Scene Names")]
    [SerializeField] string mainMenuSceneName;
    [SerializeField] string gameplaySceneName;

    [Header("Player Parameters")]
    [SerializeField] Vector3 paInitialPosition = Vector3.zero;

    [SerializeField] bool playingAsPA;

    bool titleScreenShown = false;
    bool inGameplay = false;
    ControlModes controlMode;

    GameplayController gameplayController;

    public bool TitleScreenShown { get { return titleScreenShown; } }
    public bool InGameplay { get { return inGameplay; } }
    public string MainMenuScene { get { return mainMenuSceneName; } }
    public string GameplayScene { get { return gameplaySceneName; } }
    public string CurrentScene { get { return SceneManager.GetActiveScene().name; } }
    public ControlModes ControlMode
    {
        set
        {
            controlMode = value;

            switch (controlMode)
            {
                case ControlModes.UI:
                    Cursor.lockState = CursorLockMode.Confined;
                    Cursor.visible = true;
                    break;
                case ControlModes.Gameplay:
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    break;
                default:
                    break;
            }

            OnControlModeChanged?.Invoke(controlMode);
        }

        get { return controlMode; }
    }

    static public event Action<string> OnSceneLoaded;
    static public event Action<ControlModes> OnControlModeChanged;

    void OnEnable()
    {
        NetworkManager.OnMatchBegun += SetPlayingAsPA;
        SceneManager.sceneLoaded += CheckLoadedScene;

        TitleScreen.OnTitleScreenClosed += OnTitleScreenClosed;
        UIManager_MainMenu.OnExit += Exit;

        GameplayController.OnLevelEnd += () => inGameplay = false;
    }

    void OnDisable()
    {
        NetworkManager.OnMatchBegun -= SetPlayingAsPA;
        SceneManager.sceneLoaded -= CheckLoadedScene;

        TitleScreen.OnTitleScreenClosed -= OnTitleScreenClosed;
        UIManager_MainMenu.OnExit -= Exit;

        GameplayController.OnLevelEnd -= () => inGameplay = false;
    }

    void SetPlayingAsPA(bool _playingAsPA) => playingAsPA = _playingAsPA;

    void CheckLoadedScene(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.name == MainMenuScene) ControlMode = ControlModes.UI;
        else if (scene.name == GameplayScene)
        {
            inGameplay = true;
            ControlMode = ControlModes.Gameplay;

            gameplayController = GameplayController.Get();
            gameplayController.playingAsPA = playingAsPA;
        }

        OnSceneLoaded?.Invoke(scene.name);
    }

    void OnTitleScreenClosed() => titleScreenShown = true;

    #region Scene Flow
    void Exit() => Application.Quit();
    #endregion
}