using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    Material mat;


    private void Awake()
    {
        mat = GetComponent<MeshRenderer>().material;
    }
    // Start is called before the first frame update
    public void OnPlayerWatching()
    {
        mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, 0.5f);
    }
}