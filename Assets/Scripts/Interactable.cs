using System.Collections;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField, Range(0.0f, 1.0f)] float hoverFadeAlpha = 0.75f;
    [SerializeField, Range(0.0f, 1.0f)] float clickFadeAlpha = 1.0f;
    [SerializeField] float clickFadeDuration = 0.5f;

    bool fadedOnClick = false;
    public bool active = true;
    
    Material material;

    protected virtual void Awake()
    {
        material = GetComponent<MeshRenderer>().material;
    }

    public virtual void OnPlayerHovering()
    {
        if (active && !fadedOnClick) material.color = new Color(material.color.r, material.color.g, material.color.b, hoverFadeAlpha);
    }

    public virtual void OnPlayerNotHovering()
    {
        if (!fadedOnClick) material.color = new Color(material.color.r, material.color.g, material.color.b, 1.0f);
    }

    public virtual void OnClicked()
    {
        if (active && !fadedOnClick) StartCoroutine(FadeOnClick());
    }

    IEnumerator FadeOnClick()
    {
        fadedOnClick = true;
        material.color = new Color(material.color.r, material.color.g, material.color.b, clickFadeAlpha);
    
        yield return new WaitForSeconds(clickFadeDuration);
    
        material.color = new Color(material.color.r, material.color.g, material.color.b, 1.0f);
        fadedOnClick = false;
    }
}