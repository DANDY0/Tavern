using GrandDevs.Tavern.Common;
using System;

namespace GrandDevs.Tavern
{
    public interface IScenesManager
    {
        event Action<Enumerators.AppState> SceneForAppStateWasLoadedEvent;

        bool IsLoadedScene { get; set; }

        void ChangeScene(Enumerators.AppState appState); 
    }
}