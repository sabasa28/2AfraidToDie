using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : PersistentMonoBehaviourSingleton<GameManager>
{
    [Header("Scene Names")]
    [SerializeField] string MainMenuSceneName;
    [SerializeField] string GameplaySceneName;

    [Header("Player Parameters")]
    [SerializeField] Vector3 paInitialPosition = Vector3.zero;

    [SerializeField] bool playingAsPA;

    GameplayController gameplayController;

    public string MainMenuScene { get { return MainMenuSceneName; } }
    public string GameplayScene { get { return GameplaySceneName; } }

    void OnEnable()
    {
        NetworkManager.OnMatchBegun += SetPlayingAsPA;
        SceneManager.sceneLoaded += CheckLoadedScene;
        UIManager_MainMenu.OnExit += Exit;
        UIManager_Gameplay.OnGoToMainMenu += GoToMainMenu;
    }

    void OnDisable()
    {
        NetworkManager.OnMatchBegun -= SetPlayingAsPA;
        SceneManager.sceneLoaded -= CheckLoadedScene;
        UIManager_MainMenu.OnExit -= Exit;
        UIManager_Gameplay.OnGoToMainMenu -= GoToMainMenu;
    }

    void SetPlayingAsPA(bool _playingAsPA) => playingAsPA = _playingAsPA;

    void CheckLoadedScene(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.name == GameplayScene)
        {
            gameplayController = GameplayController.Get();
            gameplayController.playingAsPA = playingAsPA;
        }
    }

    #region Scene Flow
    void Exit() => Application.Quit();

    void GoToMainMenu() => SceneManager.LoadScene(MainMenuScene);
    #endregion
}