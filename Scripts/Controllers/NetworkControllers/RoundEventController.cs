using System;
using System.IO;
using GrandDevs.Networking;
using UnityEngine;
using static GrandDevs.Tavern.Common.Enumerators;

namespace GrandDevs.Tavern
{
    public class RoundEventController : IController
    {
        private INetworkManager _networkManager;
        private IGameplayManager _gameplayManager;
        private GameplayController _gameplayController;
        public event Action<RoundStartedData> OnRoundStarted;
        public event Action<RoundProcessingData> OnRoundProcessing;
        public event Action OnRoundEnded;
        public event Action<int> OnTurnCounterChanged;
        
        private RoundState _roundState = RoundState.None;

        private int _roundCounter = 0;

            public RoundEventController(GameplayManager gameplayManager)
        {
            _gameplayManager = gameplayManager;
        }

        private void ConnectedEventHandler() 
        {
            _networkManager.SubscribeSocketWithParam<GameEvent>
                (Enumerators.NetworkEventType.GameplayEvent, GameEventReceivedEventHandler);
        }

        private void DisconnectedEventHandler() 
        {
            _networkManager.UnSubscribeSocketWithParam<GameEvent>
                (Enumerators.NetworkEventType.GameplayEvent, GameEventReceivedEventHandler);
        }

        private void GameEventReceivedEventHandler(GameEvent gameEvent)
        {
            switch (gameEvent.type)
            {
                case Enumerators.GameplayEventType.RoundStarted:
                {
                    _roundState = RoundState.RoundStart;
                    var data = gameEvent.GetGameData<RoundStartedData>();
                    if(data.grid != null && data.grid.Count > 0)
                        _gameplayManager.Board.UpdateGrid(data.grid, _roundCounter > 1);
                    OnRoundStarted?.Invoke(data);
                    OnTurnCounterChanged?.Invoke(++_roundCounter);
                }
                    break;
                case Enumerators.GameplayEventType.RoundProcessing:
                {
                    _roundState = RoundState.RoundProcessing;
                    var data = gameEvent.GetGameData<RoundProcessingData>();
                    _gameplayManager.Board.UpdateGrid(data.grid, false);
                    OnRoundProcessing?.Invoke(data);
                    LogDataToFile(gameEvent);
                }
                    break;
                case Enumerators.GameplayEventType.RoundEnded:
                { 
                    _roundState = RoundState.RoundEnd;
                    OnRoundEnded?.Invoke(); 
                }
                    break;
                case Enumerators.GameplayEventType.GameEnded:
                { 
                    // TODO: implement correct logic.
                }
                    break;
            }
        }
        
        private void LogDataToFile(GameEvent data)
        {
            string path = Path.Combine(Application.persistentDataPath, "gameDataLog.txt");
            string logEntry = $"ROUND: {_roundCounter}\n" +
                              $"Event Type: {data.type}\n"
                              + $"Data: {data.data}\n"
                              + "------------------------------\n";

            File.AppendAllText(path, logEntry);
        }

        public void Init()
        {
            _networkManager = GameClient.Get<INetworkManager>();
            _gameplayManager.GetController<ConnectionController>().OnConnect += ConnectedEventHandler;
            _gameplayManager.GetController<ConnectionController>().OnDisconnect += DisconnectedEventHandler;
        }

        public void Update()
        {
        }

        public void Dispose()
        {
            _gameplayManager.GetController<ConnectionController>().OnConnect -= ConnectedEventHandler;
            _gameplayManager.GetController<ConnectionController>().OnDisconnect -= DisconnectedEventHandler;
        }

        public void ResetAll()
        {
            _roundCounter = 0;
        }

        public RoundState GetRoundState() => _roundState;
        
      
    } 
}