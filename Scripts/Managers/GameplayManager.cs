using System;
using System.Collections.Generic;
using GrandDevs.Networking;
using UnityEngine;
using Enumerators = GrandDevs.Tavern.Common.Enumerators;

namespace GrandDevs.Tavern
{
    public class GameplayManager : IService, IGameplayManager
    {
        // public event Action<Networking.Enumerators.GameplayEventType> RoundStateChangedEvent;

        public event Action GameplayStartedEvent;
        public event Action GameplayEndedEvent;

        private List<IController> _controllers;

        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private IDataManager _dataManager;
        
        private INetworkManager _networkManager;
        
        public bool IsGameplayStarted { get; private set; }
        public bool IsGameplayPaused { get; private set; }
        public GameplayData GameplayData { get; private set; }
        public GameConfig GameConfig { get; private set; }
        // public Inventory Inventory { get; set; }
        
        public Board Board { get; set; }


        public void Dispose()
        {
            GetController<GameEventController>().OnGetGameConfig -= GetGameConfigEventHandler;
            _dataManager.DataLoadedEvent -= DataLoadedEventHandler;
            // Inventory.Dispose();
            foreach (var item in _controllers)
                item.Dispose();
        }

        public void Init()
        {
            _controllers = new List<IController>()
            {
                new ConnectionController(this),
                new GameEventController(this),
                new RoomController(this),
                new RoundEventController(this),
                
                new GameplayController(),
                new BoardInputController(),
                new RoundActionsController(),
                new DeadPlayersController(),
                new RoundInfoController(),
                new CameraController(),
                new PlayerCamerasController(),
                new ModelInputController(),
            };
            
            foreach (var item in _controllers)
                item.Init();

            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _networkManager = GameClient.Get<INetworkManager>();

            GameplayData = _loadObjectsManager.GetObjectByPath<GameplayData>("Data/GameplayData");
            
            Application.targetFrameRate = GameplayData.targetFrameRate;

            _dataManager.DataLoadedEvent += DataLoadedEventHandler;
            GetController<GameEventController>().OnGetGameConfig += GetGameConfigEventHandler;
        }
        
        private void GetGameConfigEventHandler(GameConfig gameConfig) => GameConfig = gameConfig;

        public void Update()
        {
            foreach (var item in _controllers)
                item.Update();
        }

        public T GetController<T>() where T : IController
        {
            foreach (var item in _controllers)
            {
                if (item is T)
                {
                    return (T)item;
                }
            }

            throw new Exception("Controller " + typeof(T).ToString() + " have not implemented");
        }

        public void StartGameplay()
		{
            if (IsGameplayStarted)
                return;
            
            IsGameplayPaused = false;
            IsGameplayStarted = true;
            
            GameClient.Get<IScenesManager>().SceneForAppStateWasLoadedEvent += SceneForAppStateWasLoadedEventHandler;

            GameClient.Get<IScenesManager>().ChangeScene(Enumerators.AppState.Game);
            var uiManager = GameClient.Get<IUIManager>();
            uiManager.DrawPopup<LoadingPopup>();
            
        }

        private void SceneForAppStateWasLoadedEventHandler(Enumerators.AppState state)
        {
            GameClient.Get<IScenesManager>().SceneForAppStateWasLoadedEvent -= SceneForAppStateWasLoadedEventHandler;

            if(state == Enumerators.AppState.Game)
            {
                Board = new Board();

                GameplayStartedEvent?.Invoke();
                
                _networkManager.SendGameEvent(new Networking.ClientGameplayEvent<Networking.PlayerReadyForGameData>()
                {
                    data = new GrandDevs.Networking.PlayerReadyForGameData(),
                    type = Networking.Enumerators.GameplayEventType.PlayerReadyForGame
                });
            }
        }

        public void StopGameplay()
		{
            if (!IsGameplayStarted)
                return;

            foreach (var item in _controllers)
                item.ResetAll();

            Board.Dispose();

            IsGameplayStarted = false;
            IsGameplayPaused = false;

            _uiManager.DrawPopup<LoadingPopup>();
            GameClient.Get<IScenesManager>().ChangeScene(Enumerators.AppState.Main);

            GameplayEndedEvent?.Invoke();
        }

        public void SetPauseStatusOfGameplay(bool status)
		{
            if (!IsGameplayStarted || IsGameplayPaused == status)
                return;

            IsGameplayPaused = status;
        }

        private void DataLoadedEventHandler()
        {
        }
    }
}