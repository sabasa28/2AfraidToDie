using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ArrowButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] GameObject arrowContainer = null;
    [SerializeField] GameObject leftArrow = null;
    [SerializeField] GameObject rightArrow = null;
    [Space]
    [SerializeField] float arrowHeightFactor = 1.0f;

    RectTransform rectTransform;
    RectTransform leftArrowRT;
    RectTransform rightArrowRT;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        leftArrowRT = leftArrow.GetComponent<RectTransform>();
        rightArrowRT = rightArrow.GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        arrowContainer.SetActive(true);

        float size = rectTransform.sizeDelta.y * arrowHeightFactor;
        Vector2 sizeDelta = new Vector2(size, size);
        leftArrowRT.sizeDelta = sizeDelta;
        rightArrowRT.sizeDelta = sizeDelta;
    }

    public void OnPointerExit(PointerEventData eventData) => arrowContainer.SetActive(false);
}