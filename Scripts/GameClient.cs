namespace GrandDevs.Tavern
{
    public class GameClient : ServiceLocatorBase
    {
        private static object _sync = new object();

        private static GameClient _Instance;
        public static GameClient Instance
        {
            get
            {
                if (_Instance == null)
                {
                    lock (_sync)
                    {
                        _Instance = new GameClient();
                    }
                }
                return _Instance;
            }
        }

        internal GameClient() : base()
        {
            AddService<INetworkManager>(new NetworkManager());
            AddService<IDataManager>(new DataManager());
            AddService<ILocalizationManager>(new LocalizationManager());
            AddService<ISoundManager>(new SoundManager());
            AddService<ILoadObjectsManager>(new LoadObjectsManager());
            AddService<IUserProfileManager>(new UserProfileManager());
            AddService<IUIManager>(new UIManager());
            AddService<IAppStateManager>(new AppStateManager());
            AddService<IScenesManager>(new ScenesManager());
            AddService<IGameplayManager>(new GameplayManager());
            AddService<IInputManager>(new InputManager());
        }

        public static T Get<T>()
        {
            return Instance.GetService<T>();
        }
    }
}