using System;
using GrandDevs.Networking;
using static GrandDevs.Tavern.Common.Enumerators;

namespace GrandDevs.Tavern
{
    public class GameEventController: IController
    {
        private INetworkManager _networkManager;
        private IGameplayManager _gameplayManager;
        private GameplayController _gameplayController;
        private RoundActionsController _roundActionsController;

        public event Action<GameConfig> OnGetGameConfig;
        public event Action<GridInitData> OnGridInit;
        public event Action<PlayerJoinedData> OnPlayerJoined;
        public event Action<PlayersUpdatedData> OnPlayersUpdated;
        public event Action<PlayerDisconnectedData> OnPlayerDisconnected;


        public GameEventController(GameplayManager gameplayManager)
        {
            _gameplayManager = gameplayManager;
        }
        
        public void Init()
        {
            _networkManager = GameClient.Get<INetworkManager>();
            _roundActionsController = _gameplayManager.GetController<RoundActionsController>();
            _gameplayManager.GetController<ConnectionController>().OnConnect += ConnectedEventHandler;
            _gameplayManager.GetController<ConnectionController>().OnDisconnect += DisconnectEventHandler;
        }
        
        private void GameEventReceivedEventHandler(GameEvent gameEvent)
        {
            switch(gameEvent.type)
            {
                case Enumerators.GameplayEventType.GetGameConfig:
                {
                    // Debug.LogWarning($"GET GAME CONFIG");
                    GameConfig data = gameEvent.GetGameData<GameConfig>(Enumerators.SerializationToolType.NewtonsoftJson);
                    OnGetGameConfig?.Invoke(data);
                }
                    break;
                case Enumerators.GameplayEventType.GridInit:
                {
                    // Debug.LogWarning($"GRID INIT");
                    var data = gameEvent.GetGameData<GridInitData>();
                    _gameplayManager.Board.InitGrid(data.grid);
                    OnGridInit?.Invoke(data);
                } 
                    break;
                case Enumerators.GameplayEventType.PlayerJoined:
                {
                    PlayerJoinedData data = gameEvent.GetGameData<PlayerJoinedData>(Enumerators.SerializationToolType.NewtonsoftJson);
                    // Debug.LogWarning($"PLAYER JOINED  {data.id}");
                    
                    OnPlayerJoined?.Invoke(data);
                    
                }
                    break;
                case Enumerators.GameplayEventType.PlayerDisconnected:
                {
                    PlayerDisconnectedData data = gameEvent.GetGameData<PlayerDisconnectedData>(Enumerators.SerializationToolType.NewtonsoftJson);
                    // Debug.LogWarning($"PLAYERS Disconnected  {data.id}");
                    OnPlayerDisconnected?.Invoke(data);
                }
                    break;
                case Enumerators.GameplayEventType.PlayersUpdated:
                {
                    PlayersUpdatedData data = gameEvent.GetGameData<PlayersUpdatedData>(Enumerators.SerializationToolType.NewtonsoftJson);
                    // Debug.LogWarning($"PLAYERS UPDATED   {data.players}");
                    OnPlayersUpdated?.Invoke(data);
                }
                    break;

                case Enumerators.GameplayEventType.GameStarted:
                {
                    // Debug.LogWarning($"GAME STARTED");
                    GameClient.Get<IAppStateManager>().ChangeAppState(AppState.Game);
                    var uiManager = GameClient.Get<IUIManager>();
                    uiManager.HidePopup<MatchFoundedPopup>();

                    _networkManager.SendGameEvent(new ClientGameplayEvent<GetGameConfigData>()
                    {
                        data = new GetGameConfigData(),
                        type = Enumerators.GameplayEventType.GetGameConfig
                    });
                }
                    break;
                case Enumerators.GameplayEventType.GameEnded:
                {
                    GameClient.Get<IUIManager>().DrawPopup<GameFinishedPopup>(true);
                }
                    break;
            }
        }

        public void Update()
        {
            
        }

        public void Dispose()
        {
            _gameplayManager.GetController<ConnectionController>().OnConnect -= ConnectedEventHandler;
            _gameplayManager.GetController<ConnectionController>().OnDisconnect -= DisconnectEventHandler;
        }

        public void ResetAll()
        {
            
        }
        
        private void ConnectedEventHandler()
        {
            _networkManager.SubscribeSocketWithParam<GameEvent>
                (Enumerators.NetworkEventType.GameplayEvent, GameEventReceivedEventHandler);
            
            _gameplayController = _gameplayManager.GetController<GameplayController>();
        }

        private void DisconnectEventHandler()
        {
            _networkManager.UnSubscribeSocketWithParam<GameEvent>
                (Enumerators.NetworkEventType.GameplayEvent, GameEventReceivedEventHandler);
        }

        public void LeaveGameplay()
        {
            _networkManager.Disconnect();
        }
    }
}