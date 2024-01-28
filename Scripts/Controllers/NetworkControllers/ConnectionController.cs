using System;
using GrandDevs.Networking;

namespace GrandDevs.Tavern
{
    public class ConnectionController: IController
    {
        private readonly IGameplayManager _gameplayManager;
        private INetworkManager _networkManager;
        private IAppStateManager _appStateManager;
        private GameEventController _gameEventController;
        private RoomController _roomController;
        private RoundEventController _roundEventController;

        private RoomInfo _room;

        public event Action OnConnect; 
        public event Action OnDisconnect; 

        public ConnectionController(GameplayManager gameplayManager)
        {
            _gameplayManager = gameplayManager;
        }

        public void FindFreeRoomAndJoinGame()
        {
            _networkManager.SubscribeSocketWithoutParam
                (Enumerators.NetworkEventType.Connect, ConnectedEventHandler);
            _networkManager.Connect();
        }
        
        private void ConnectedEventHandler()
        {
            _networkManager.UnSubscribeSocketWithoutParam
                ( Enumerators.NetworkEventType.Connect, ConnectedEventHandler);;
            
            _networkManager.SubscribeSocketWithoutParam
                (Enumerators.NetworkEventType.Disconnect ,DisconnectedEventHandler);
            
            OnConnect?.Invoke();
            
            _networkManager.FindAndJoinRoom();
        }
        

        private void DisconnectedEventHandler()
        {
            _networkManager.UnSubscribeSocketWithoutParam
                (Enumerators.NetworkEventType.Disconnect, DisconnectedEventHandler);
            
            OnDisconnect?.Invoke();
            
            _appStateManager.ChangeAppState(GrandDevs.Tavern.Common.Enumerators.AppState.Main);
        }
        
        public void Disconnect()
        {
            _networkManager.Disconnect();
        }

        public void Init()
        {
            _appStateManager = GameClient.Get<IAppStateManager>();
            _networkManager = GameClient.Get<INetworkManager>();
            _gameEventController = _gameplayManager.GetController<GameEventController>();
            _roomController = _gameplayManager.GetController<RoomController>();
            _roundEventController = _gameplayManager.GetController<RoundEventController>();
        }

        public void Update()
        {
        }

        public void Dispose()
        {
        }

        public void ResetAll()
        {
        }
    }
}

