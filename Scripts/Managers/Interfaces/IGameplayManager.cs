using GrandDevs.Tavern.Common;
using System;
using System.Collections.Generic;
using GrandDevs.Networking;
using UnityEngine;

namespace GrandDevs.Tavern
{
    public interface IGameplayManager
    {
        event Action GameplayStartedEvent;
        event Action GameplayEndedEvent;

        bool IsGameplayStarted { get; }
        bool IsGameplayPaused { get; }
        GameplayData GameplayData { get; }
        Board Board { get; set; }
        GameConfig GameConfig { get; }
        T GetController<T>() where T : IController;

        void StartGameplay();
        void StopGameplay();
        void SetPauseStatusOfGameplay(bool status);
    }
}