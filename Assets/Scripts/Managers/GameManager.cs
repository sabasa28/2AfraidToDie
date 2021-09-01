using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourSingleton<GameManager>
{
    [Header("Player Parameters")]
    [SerializeField] Vector3 paInitialPosition = Vector3.zero;

    [SerializeField] bool playingAsPA;

    GameplayController gameplayController;
    Player player;

    void OnEnable()
    {
        SceneManager.sceneLoaded += CheckLoadedScene;
        UIManager_MainMenu.onPlay += GoToGameplay;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= CheckLoadedScene;
        UIManager_MainMenu.onPlay -= GoToGameplay;
    }

    void CheckLoadedScene(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.name == "Gameplay")
        {
            gameplayController = GameObject.Find("Gameplay Controller").GetComponent<GameplayController>();
            player = GameObject.Find("Player").GetComponent<Player>();

            gameplayController.playingAsPA = playingAsPA;

            if (playingAsPA) player.transform.position = paInitialPosition;
            else player.transform.position = new Vector3(paInitialPosition.x, paInitialPosition.y, -paInitialPosition.z);
        }
    }

    public void GoToGameplay(bool playAsPA)
    {
        playingAsPA = playAsPA;
        SceneManager.LoadScene("Gameplay");
    }
}