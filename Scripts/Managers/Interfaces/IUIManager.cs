using System;
using UnityEngine;
using UnityEngine.UI;

namespace GrandDevs.Tavern
{
    public interface IUIManager
    {
        GameObject Canvas { get; set; }
        CanvasScaler CanvasScaler { get; set; }
        IUIElement CurrentPage { get; set; }
        void SetPage<T>(bool hideAll = false) where T : IUIElement;
        void DrawPopup<T>(object message = null, bool setMainPriority = false) where T : IUIPopup;
        void HidePopup<T>() where T : IUIPopup;
        T GetPopup<T>() where T : IUIPopup;
        T GetPage<T>() where T : IUIElement;

        void HideAllPages();
        void HideAllPopups();
        void FadeScreen(GameObject screen, bool fadeIn, Action callback = null);
    }
}