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
        if (PhotonNetwork.IsMasterClient)                               //ya no es necesario creo
        {                                                               //ya no es necesario creo
            foreach (Difference diff in posibleDifferences)             //ya no es necesario creo
            {                                                           //ya no es necesario creo
                diff.SetTransformAsDifference(false);                   //ya no es necesario creo
            }                                                           //ya no es necesario creo
        }                                                               //ya no es necesario creo
    }

    public void SetDifferences(int[] differenceIndex, bool[] baseDifferencesState)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < differenceIndex.Length; i++)
            {
                baseDifferencesState[differenceIndex[i]] = !baseDifferencesState[differenceIndex[i]];
            }
        }

        for (int i = 0; i < differenceIndex.Length; i++)
        {
            Difference diff = posibleDifferences[differenceIndex[i]];
            Debug.Log(diff.name);
            differences.Add(diff);
        }

        for (int i = 0; i < posibleDifferences.Count; i++)
        {
            posibleDifferences[i].SetTransformAsDifference(baseDifferencesState[i]);
        }
 
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
