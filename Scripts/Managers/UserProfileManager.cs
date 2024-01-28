using System;
using System.Threading.Tasks;
using External.Essentials.GrandDevs.SocketIONetworking.Scripts;

namespace GrandDevs.Tavern
{
    public class UserProfileManager: IService, IUserProfileManager
    {
        public APIModel.GetUserProfileResponse.UserData UserProfile { get; set; }
        public MapActionsHandler MapActionsHandler { get; private set; }
        public Inventory Inventory { get; private set; }
        public int StaminaValue { get; private set; }

        private IGameplayManager _gameplayManager;
        private INetworkManager _networkManager;
        private IDataManager _dataManager;
        private ApiRequestHandler _apiRequestHandler;


        private APIModel.GameConfigData _gameConfigData;
        private int _minStartValue = 20;

        public async void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _networkManager = GameClient.Get<INetworkManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _apiRequestHandler = _networkManager.APIRequestHandler;
            _gameplayManager.GameplayStartedEvent += GameplayStartedEventHandler;
            _apiRequestHandler.GetApiConfigEvent += GetApiConfigHandler;
   
            Inventory = new Inventory();
            MapActionsHandler = new MapActionsHandler();
            StaminaValue = 100;

            await _networkManager.APIRequestHandler.SignInAndStoreToken();
        }

        private void GetApiConfigHandler(APIModel.GameConfigData data)
        {
            _gameConfigData = data;
        }

        public void Update()
        {
        }

        private void GameplayStartedEventHandler()
        {
            StaminaValue -= 20;
        }
        
        public bool IsLocalPlayer(string userID)
        {
            return UserProfile.id == userID;
        }

        public bool IsStaminaEnough()
        {
            return StaminaValue >= _minStartValue;
        }

        public void Dispose()
        {
            _gameplayManager.GameplayStartedEvent -= GameplayStartedEventHandler;
            Inventory.Dispose();
            Inventory = null;
        }
    }
}