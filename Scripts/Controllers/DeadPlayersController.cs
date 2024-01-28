using System.Collections.Generic;
using System.Linq;
using GrandDevs.Networking;
using Enumerators = GrandDevs.Tavern.Common.Enumerators;

namespace GrandDevs.Tavern
{
    public class DeadPlayersController: IController
    {
        private readonly IGameplayManager _gameplayManager;
        private readonly INetworkManager _networkManager;
        private RoundEventController _roundEventsController;
        private GameplayController _gameplayController;
        private SpectatorsArea _spectatorsArea;

        public DeadPlayersController()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _networkManager = GameClient.Get<INetworkManager>();
        }
        
        public void Init()
        {
             _roundEventsController = _gameplayManager.GetController<RoundEventController>();
             _gameplayController = _gameplayManager.GetController<GameplayController>();
             _gameplayManager.GameplayStartedEvent += GameplayStartedEventHandler;
            _roundEventsController.OnRoundStarted += RoundStartedEventHandler;
            _roundEventsController.OnRoundEnded += RoundEndedEventHandler;
        }

        private void GameplayStartedEventHandler() => _spectatorsArea = new SpectatorsArea();

        public void Update()
        {
        }

        public void Dispose()
        {
            _roundEventsController.OnRoundStarted -= RoundStartedEventHandler;
            _roundEventsController.OnRoundEnded -= RoundEndedEventHandler;
        }

        public void ResetAll()
        {
            _spectatorsArea.Dispose();
            _spectatorsArea = null;
        }


        private void RoundStartedEventHandler(RoundStartedData startedData)
        {
            /*
            List<int> allPlayerIDs = _gameplayController.GetPlayers()
                .Select(p => p.GetPlayerData().PlayerID).ToList();

            List<int> playerIDsInGrid = startedData.grid
                .Where(gridItem => gridItem.element != null 
                                   && gridItem.element.type == Enumerators.UnitType.Character.ToString())
                .Select(gridItem => gridItem.element.id).ToList();

            var deadPlayerIDs = allPlayerIDs.Except(playerIDsInGrid);

            List<DwarfPlayer> deadPlayers = new List<DwarfPlayer>();

            foreach (var deadPlayerID in deadPlayerIDs)
                deadPlayers.Add(_gameplayController.GetPlayerByID(deadPlayerID));

            foreach (var player in deadPlayers)
                _spectatorsArea.TryPlaceInSpectatorsArea(player);
        */
        }

        private void RoundEndedEventHandler()
        {
            foreach (var player in _gameplayController.GetPlayers().Where(player => player.IsPlayerDead()))
                _spectatorsArea.TryPlaceInSpectatorsArea(player);
        }

        public void OnPlayerDead(Player player) => _spectatorsArea.TryPlaceInSpectatorsArea(player);
    }
}