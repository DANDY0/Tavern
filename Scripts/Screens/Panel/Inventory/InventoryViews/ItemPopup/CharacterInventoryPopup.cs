using UnityEngine;

public class CharacterInventoryPopup : BaseItemPopup
{
    public CharacterInventoryPopup(GameObject selfObject) : base(selfObject)
    {
    }

    public override void SetPosition(RectTransform itemRectTransform)
    {
        base.SetPosition(itemRectTransform);
    }
}