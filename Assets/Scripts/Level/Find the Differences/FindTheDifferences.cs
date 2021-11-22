using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindTheDifferences : MonoBehaviour
{
    List<Difference> differences;
    Transform interactablesParent;
    [SerializeField] Transform interactablesParentA;
    [SerializeField] Transform interactablesParentB;

    private void Awake()
    {
        differences = new List<Difference>();
    }
    public void SetSide(bool isSideA)
    {
        interactablesParent = isSideA ? interactablesParentA : interactablesParentB;
    }
    public int GetInteractablesNumber()
    {
        return interactablesParent.childCount;
    }

    public void SetDifference(int differenceIndex)
    {
        Debug.Log(interactablesParent.GetChild(differenceIndex).name);
        Debug.Log(interactablesParent.GetChild(differenceIndex).GetComponent<Difference>());
        Difference diff = interactablesParent.GetChild(differenceIndex).GetComponent<Difference>();
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
