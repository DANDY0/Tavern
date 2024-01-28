using UnityEngine;

public class MainInventoryPopup : BaseItemPopup
{
    private RectTransform _rectTransform;

    public MainInventoryPopup(GameObject selfObject) : base(selfObject)
    {
    }

    public override void SetPosition(RectTransform itemRectTransform)
    {
        base.SetPosition(itemRectTransform);
    }
}