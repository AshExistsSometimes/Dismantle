using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SVImageControl : MonoBehaviour, IDragHandler, IPointerClickHandler
{
    [SerializeField]
    private Image pickerImage;

    private RawImage SVImage;

    private ColourPickerControl CC;

    private RectTransform rectTransform, pickerTransform;

    

    private void Awake()
    {
        SVImage = GetComponent<RawImage>();
        CC = FindObjectOfType<ColourPickerControl>();
        rectTransform = GetComponent<RectTransform>();

        pickerTransform = pickerImage.GetComponent<RectTransform>();
        pickerTransform.position = new Vector2(-(rectTransform.sizeDelta.x * 0.5f), -(rectTransform.sizeDelta.y * 0.5f));
    }

    void UpdateColour(PointerEventData eventData)
    {
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPos
        );

        float halfWidth = rectTransform.rect.width * 0.5f;
        float halfHeight = rectTransform.rect.height * 0.5f;

        // Clamp strictly to the rect
        localPos.x = Mathf.Clamp(localPos.x, -halfWidth, halfWidth);
        localPos.y = Mathf.Clamp(localPos.y, -halfHeight, halfHeight);

        // Normalize (0–1)
        float xNorm = (localPos.x + halfWidth) / (halfWidth * 2f);
        float yNorm = (localPos.y + halfHeight) / (halfHeight * 2f);

        pickerTransform.localPosition = localPos;

        // Optional visual feedback (value usually mapped vertically)
        pickerImage.color = Color.HSVToRGB(0f, 0f, 1f - yNorm);

        CC.SetSV(xNorm, yNorm);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateColour(eventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        UpdateColour(eventData);
    }

}
