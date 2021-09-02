using System.Collections;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    bool fadedOnClick = false;
    float clickFadeDuration = 0.5f;
    public bool active = true;

    Material mat;

    void Awake()
    {
        mat = GetComponent<MeshRenderer>().material;
    }

    public void OnPlayerWatching()
    {
        if (active && !fadedOnClick) mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, 0.75f);
    }

    public void OnPlayerNotWatching()
    {
        if (!fadedOnClick) mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, 1.0f);
    }

    public void OnClicked()
    {
        if (active && !fadedOnClick) StartCoroutine(FadeOnClick());
    }

    IEnumerator FadeOnClick()
    {
        fadedOnClick = true;
        mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, 1.0f);

        yield return new WaitForSeconds(clickFadeDuration);

        mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, 1.0f);
        fadedOnClick = false;
    }
}