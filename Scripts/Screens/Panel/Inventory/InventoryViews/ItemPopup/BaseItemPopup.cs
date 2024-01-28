using TMPro;
using UnityEngine;

public class BaseItemPopup
{
    protected GameObject _selfObject;
    protected TextMeshProUGUI _itemName;
    protected TextMeshProUGUI _itemDescription;
    protected RectTransform _rectTransform;
        
    private Vector2 _panelSize;
    private readonly float _offsetForBottomItems = -300;

    public BaseItemPopup(GameObject selfObject)
    {
        _selfObject = selfObject;
        _itemName = _selfObject.transform.Find("Text_ItemName").GetComponent<TextMeshProUGUI>();
        _itemDescription = _selfObject.transform.Find("Text_ItemDescription").GetComponent<TextMeshProUGUI>();
        _rectTransform = _selfObject.GetComponent<RectTransform>();
        _panelSize = _rectTransform.rect.size;
        Hide();
    }

    public virtual void Show(string name, string description)
    {
        _itemName.text = name;
        _itemDescription.text = description;
        _selfObject.SetActive(true);
    }

    public void Hide()
    {
        _selfObject.SetActive(false);
    }
    
    public virtual void SetPosition(RectTransform itemRectTransform) 
    {
        var localPoint = GetLocalCanvasPosition(itemRectTransform);
        Vector2 offset = CalculatePopupOffset(localPoint, itemRectTransform);
        _rectTransform.anchoredPosition = localPoint + offset;
    }

    private Vector2 CalculatePopupOffset(Vector2 localPoint, RectTransform itemRectTransform)
    {
        float localOffsetY = DetermineVerticalOffset(localPoint, itemRectTransform);
        float localOffsetX = DetermineHorizontalOffset(localPoint);
        return new Vector2(localOffsetX, localOffsetY);
    }

    private float DetermineVerticalOffset(Vector2 localPoint, RectTransform itemRectTransform)
    {
        return localPoint.y < _offsetForBottomItems ? itemRectTransform.rect.height : -_panelSize.y;
    }

    private float DetermineHorizontalOffset(Vector2 localPoint)
    {
        var parentRect = _selfObject.transform.parent.GetComponent<RectTransform>();
        float rightEdge = parentRect.rect.width / 2;
        float leftEdge = -rightEdge;
        float bufferSpace = _panelSize.x;

        if (localPoint.x + bufferSpace > rightEdge)
            return rightEdge - (localPoint.x + bufferSpace);
    
        if (localPoint.x - bufferSpace < leftEdge) 
            return leftEdge + bufferSpace - localPoint.x;
        return 0;
    }

    private Vector2 GetLocalCanvasPosition(RectTransform itemRectTransform)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_selfObject.transform.parent.GetComponent<RectTransform>(),
            RectTransformUtility.WorldToScreenPoint(null, itemRectTransform.position),
            null,
            out localPoint);
        return localPoint;
    }
}