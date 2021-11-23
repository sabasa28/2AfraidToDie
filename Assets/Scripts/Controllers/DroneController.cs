using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneController : MonoBehaviourSingleton <DroneController>
{
    [SerializeField] Drone[] drones;
    [SerializeField] float flightHeight;

    public ButtonMissingPart SendDrone(Vector3 positionToDeliver)
    {
        int availableDrone = 0;
        for (int i = 0; i < drones.Length; i++)
        {
            if (drones[i].gameObject.activeSelf == false)
            {
                availableDrone = i;
                break;
            }
        }
        drones[availableDrone].transform.parent.position = positionToDeliver + new Vector3 (0.0f, flightHeight);
        drones[availableDrone].gameObject.SetActive(true);
        drones[availableDrone].SetStartingMovement(Drone.MovementToDo.deliverPulser);
        return drones[availableDrone].GetPulser();
    }
}
