using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FindTheDifferences : MonoBehaviour
{
    List<Difference> posibleDifferences;
    List<Difference> differences;
    Transform interactablesParent;
    [SerializeField] Transform interactablesParentA;
    [SerializeField] Transform interactablesParentB;

    private void Awake()
    {
        differences = new List<Difference>();
        posibleDifferences = new List<Difference>();
    }
    public void SetSide(bool isSideA)
    {
        Transform lastTransform = interactablesParent;
        interactablesParent = isSideA ? interactablesParentA : interactablesParentB;
        
        if (interactablesParent == lastTransform) return;
        
        posibleDifferences.Clear();
        
        for (int i = 0; i < interactablesParent.childCount; i++)
        {
            posibleDifferences.Add(interactablesParent.GetChild(i).GetComponent<Difference>()); //puede fallar si no son todas diferencias pero deberian serlo
        }
    }

    public int GetInteractablesNumber()
    {
        return posibleDifferences.Count;
    }

    public void ClearDifferences()
    {
        differences.Clear();
        if (PhotonNetwork.IsMasterClient)
        {
            foreach (Difference diff in posibleDifferences)
            {
                diff.SetTransformAsDifference(false);
            }
        }
    }

    public void SetDifference(int differenceIndex)
    {
        Debug.Log(posibleDifferences[differenceIndex].gameObject.name);
        Difference diff = posibleDifferences[differenceIndex];
        if (PhotonNetwork.IsMasterClient)
        {
            diff.SetTransformAsDifference(true);
        }
        differences.Add(diff);
    }

    public bool CheckInteractable(Difference interactableToTest)
    {
        return differences.Remove(interactableToTest);
    }

    public int GetDifferencesLeft()
    {
        return differences.Count;
    }
}
