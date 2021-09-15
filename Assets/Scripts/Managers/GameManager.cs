﻿using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : PersistentMonoBehaviourSingleton<GameManager>
{
    [Header("Player Parameters")]
    [SerializeField] Vector3 paInitialPosition = Vector3.zero;

    [SerializeField] bool playingAsPA;

    GameplayController gameplayController;
    Player player;

    void OnEnable()
    {
        SceneManager.sceneLoaded += CheckLoadedScene;

        UIManager_MainMenu.OnExit += Exit;
        UIManager_MainMenu.OnPlay += GoToGameplay;

        UIManager_Gameplay.OnGoToMainMenu += GoToMainMenu;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= CheckLoadedScene;

        UIManager_MainMenu.OnExit -= Exit;
        UIManager_MainMenu.OnPlay -= GoToGameplay;

        UIManager_Gameplay.OnGoToMainMenu -= GoToMainMenu;
    }

    void CheckLoadedScene(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.name == "Gameplay")
        {
            gameplayController = GameplayController.Get();
            player = GameObject.Find("Player").GetComponent<Player>();

            gameplayController.playingAsPA = playingAsPA;
            CharacterController playerCharacterController = player.GetComponent<CharacterController>();
            playerCharacterController.enabled = false;
            if (playingAsPA) player.transform.position = paInitialPosition;
            else player.transform.position = new Vector3(paInitialPosition.x, paInitialPosition.y, -paInitialPosition.z);
            playerCharacterController.enabled = true;
        }
    }

    #region Scene Flow
    void Exit()
    {
        Application.Quit();
    }

    void GoToMainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

    void GoToGameplay(bool playAsPA)
    {
        playingAsPA = playAsPA;
        SceneManager.LoadScene("Gameplay");
    }
    #endregion
}