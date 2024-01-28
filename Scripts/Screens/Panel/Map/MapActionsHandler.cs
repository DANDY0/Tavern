using External.Essentials.GrandDevs.SocketIONetworking.Scripts;
using GrandDevs.Tavern.Common;
using GrandDevs.Tavern.Helpers;

namespace GrandDevs.Tavern
{
    public class MapActionsHandler
    {
        private readonly IUIManager _uiManager;
        private readonly IDataManager _dataManager;
        private TravelResultPopup _travelResultPopup;
        private APIModel.MapAction _currentAction;

        public MapActionsHandler()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _dataManager.DataLoadedEvent += DataLoadedHandle;
        }

        public void SetCurrentMapAction(APIModel.MapAction action)
        {
            _currentAction = action;
        }

        public void HandleCurrentAction()
        {
            if(_currentAction !=null)
                HandleAction(_currentAction);
        }

        public void HandleAction(APIModel.MapAction action)
        {
            _travelResultPopup = _uiManager.GetPopup<TravelResultPopup>();

            Enumerators.MapActions mapAction = InternalTools.EnumFromString<Enumerators.MapActions>(action.type);
            switch (mapAction)
            {
                case Enumerators.MapActions.Battle:
                    StartBattle(action.id, mapAction);
                    break;
                case Enumerators.MapActions.Item:
                    NotifyNewItem(action.id, mapAction);
                    break;
                case Enumerators.MapActions.Quest:
                    StartQuest(action.id, mapAction);
                    break;
            }
            _uiManager.DrawPopup<TravelResultPopup>();
        }

        private void StartBattle(string battleId, Enumerators.MapActions mapAction)
        {
            _travelResultPopup.ShowButtons(mapAction, battleId);
        }

        private void NotifyNewItem(string itemId, Enumerators.MapActions mapAction)
        {
            _travelResultPopup.ShowButtons(mapAction, itemId);
        }

        private void StartQuest(string questId, Enumerators.MapActions mapAction)
        {
            _travelResultPopup.ShowButtons(mapAction, questId);
        }

        private void DataLoadedHandle()
        {
            // _travelResultPopup = _uiManager.GetPopup<TravelResultPopup>();
        }
    }
}