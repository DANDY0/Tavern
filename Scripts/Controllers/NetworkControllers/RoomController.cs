using System;
using GrandDevs.Networking;
using static GrandDevs.Tavern.Common.Enumerators;

namespace GrandDevs.Tavern
{
    public class RoomController: IController
    {
        private readonly IGameplayManager _gameplayManager;
        private INetworkManager _networkManager;
        private GameplayController _gameplayController;
        private RoomInfo _room;
        public int CurrentLevel { get; set; }
        public event Action<RoomInfo> OnJoinedRoom;

        public RoomController(GameplayManager gameplayManager)
        {
            _gameplayManager = gameplayManager;
        }

        private void ConnectedEventHandler()
        {
            _networkManager = GameClient.Get<INetworkManager>();
            _networkManager.SubscribeSocketWithParam<RoomInfo>
                (Enumerators.NetworkEventType.JoinedRoom, JoinedRoomEventHandler);
        }

        private void DisconnectEventHandler()
        {
           _networkManager.UnSubscribeSocketWithParam<RoomInfo>
                (Enumerators.NetworkEventType.JoinedRoom, JoinedRoomEventHandler);
            
            GameClient.Get<IAppStateManager>().ChangeAppState(AppState.Main);
        }
        
        private void JoinedRoomEventHandler(RoomInfo room)
        {
            _room = room;
            CurrentLevel = _room.level;
            _networkManager.UnSubscribeSocketWithParam<RoomInfo>
                (Enumerators.NetworkEventType.JoinedRoom, JoinedRoomEventHandler);
            GameClient.Get<IUIManager>().DrawPopup<MatchFoundedPopup>();
            OnJoinedRoom?.Invoke(_room);
        }

        public void Init()
        {
            _gameplayManager.GetController<ConnectionController>().OnConnect += ConnectedEventHandler;
            _gameplayManager.GetController<ConnectionController>().OnDisconnect += DisconnectEventHandler;
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
    }
}