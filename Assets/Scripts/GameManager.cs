﻿using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourSingleton<GameManager>
{
    [Header("Player Parameters")]
    [SerializeField] Vector3 paInitialPosition = Vector3.zero;

    bool playingAsPA;

    Player player;

    void OnEnable()
    {
        SceneManager.sceneLoaded += CheckLoadedScene;
    }

    void Start()
    {
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= CheckLoadedScene;
    }

    void CheckLoadedScene(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.name == "Gameplay")
        {
            Debug.Log("playing as PA: " + playingAsPA);
            player = GameObject.Find("Player").GetComponent<Player>();

            player.playingAsPA = playingAsPA;

            if (playingAsPA) player.transform.position = paInitialPosition;
            else player.transform.position = new Vector3(paInitialPosition.x, paInitialPosition.y, -paInitialPosition.z);
        }
    }

    public void Play(bool playAsPA)
    {
        playingAsPA = playAsPA;
        SceneManager.LoadScene("Gameplay");
    }
}