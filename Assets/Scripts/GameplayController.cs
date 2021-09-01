using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayController : MonoBehaviour
{
    [SerializeField] Player player;
    bool playerisPA;
    [SerializeField] float TimeToRespawnPlayer;
    [SerializeField] Floor[] playerAFloor;
    [SerializeField] Floor[] playerBFloor;
    [SerializeField] float[] checkPoints;
    int currentCheckpoint = 0;
    [SerializeField] float RespawnZPA;
    [SerializeField] float RespawnZPB;

    void Start()
    {
        playerisPA = player.playingAsPA;
        player.OnTimeEnd = OpenPlayersFloor;
        player.RespawnAtCheckpoint = RespawnPlayer;

    }

    void OpenPlayersFloor()
    {
        Floor[] floorsToOpen = playerAFloor;
        if (!playerisPA) floorsToOpen = playerBFloor;

        for (int i = 0; i < floorsToOpen.Length; i++)
        {
            floorsToOpen[i].Open();
        }
    }

    void RespawnPlayer()
    {
        float zPosToRespawn = RespawnZPA;
        if (!playerisPA) zPosToRespawn = RespawnZPB;
        player.transform.position = new Vector3(checkPoints[currentCheckpoint], 0.0f, zPosToRespawn);
    }
}
