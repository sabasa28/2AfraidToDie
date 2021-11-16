using Photon.Pun;
using System;
using UnityEngine;

public class LevelEnd : MonoBehaviour
{
    public static event Action OnLevelEndReached;

    void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out PhotonView photonView) || !photonView.IsMine) return;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        OnLevelEndReached?.Invoke();
    }
}