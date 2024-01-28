using UnityEngine;

namespace GrandDevs.Tavern
{
    public interface IUIElement
    {
        void Init();
        void Show();
        void Hide();
        void Update();
        void Dispose();
    }
}