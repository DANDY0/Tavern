using System;
using GrandDevs.Tavern.Common;
using GrandDevs.Tavern.Helpers;

namespace GrandDevs.Tavern
{
    public sealed class AppStateManager : IService, IAppStateManager
    {
        private IUIManager _uiManager;

        private IDataManager _dataManager;

        public Enumerators.AppState AppState { get; private set; } = Enumerators.AppState.Unknown;

        public event Action<Enumerators.AppState> OnAppStateChanged;

        public void Dispose()
        {

        }

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _dataManager = GameClient.Get<IDataManager>();

            _dataManager.DataLoadedEvent += DataLoadedEventHandler;
        }

        private void DataLoadedEventHandler()
        {
           ChangeAppState(Enumerators.AppState.Main);
        }

        public void Update()
        {

        }

        public void ChangeAppState(Enumerators.AppState stateTo)
        {
            if (AppState == stateTo)
                return;

            AppState = stateTo;

            switch (stateTo)
            {
                case Enumerators.AppState.Main:
                    _uiManager.SetPage<MainPage>();
                    _uiManager.HidePopup<LoadingPopup>();
                    GameClient.Get<IGameplayManager>().StopGameplay();
                    break;
                case Enumerators.AppState.Game:
                    _uiManager.SetPage<GamePage>();
                    GameClient.Get<IGameplayManager>().StartGameplay();
                    break;
            }
            OnAppStateChanged?.Invoke(AppState);
        }
    }
}