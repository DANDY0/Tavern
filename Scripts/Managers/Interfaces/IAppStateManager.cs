using System;
using GrandDevs.Tavern.Common;

namespace GrandDevs.Tavern
{
    public interface IAppStateManager
    {
        Common.Enumerators.AppState AppState { get; }
        void ChangeAppState(Common.Enumerators.AppState stateTo);
        public event Action<Enumerators.AppState> OnAppStateChanged;
    }
}