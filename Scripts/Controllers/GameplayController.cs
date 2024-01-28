using System;
using System.Collections.Generic;
using System.Linq;
using GrandDevs.Networking;
using NUnit.Framework;
using UnityEngine;
using Enumerators = GrandDevs.Tavern.Common.Enumerators;
using Object = UnityEngine.Object;

namespace GrandDevs.Tavern
{
    public class GameplayController: IController
    {
        public event Action OnPlayersSpawned;
        
        private readonly IGameplayManager _gameplayManager;
        private readonly INetworkManager _networkManager;
        private readonly IAppStateManager _appStateManager;
        private readonly ILoadObjectsManager _loadObjectsManager;
        private readonly IUserProfileManager _userProfileManager;

        private GameEventController _gameEventsController;
        private RoomController _roomController;

        private List<Player> _players;
        private List<GameConfig.CharacterData> _charactersData;
        private List<GameConfig.CharacterPoint> _characterPoints = new List<GameConfig.CharacterPoint>();
        private Transform _parentOfPlayers;
        private Transform _parentOfLevels;

        private List<string> _characterPrefabPaths = new List<string>
        {
            {"Prefabs/Gameplay/Characters/Troll"},
            {"Prefabs/Gameplay/Characters/Bear"},
            {"Prefabs/Gameplay/Characters/Knight"},
            {"Prefabs/Gameplay/Characters/Demon"}
        };

        private bool _isPlayersSpawned;
        private int _currentLevel;

        public GameplayController()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _networkManager = GameClient.Get<INetworkManager>();
            _userProfileManager = GameClient.Get<IUserProfileManager>();
            _appStateManager = GameClient.Get<IAppStateManager>();
            _appStateManager.OnAppStateChanged += AppStateChangedEventHandler;
        }

        public void Init()
        {
            _players = new List<Player>();
            _gameEventsController = _gameplayManager.GetController<GameEventController>();
            _roomController = _gameplayManager.GetController<RoomController>();
            _gameEventsController.OnGridInit += GridInitEventHandler;
            _gameEventsController.OnPlayerJoined += PlayerJoinedEventHandler;
            _gameEventsController.OnPlayersUpdated += PlayersUpdatedEventHandler;
            _gameEventsController.OnPlayerDisconnected += PlayerDisconnectedEventHandler;
            _gameEventsController.OnGetGameConfig += GetGameConfigEventHandler;
            _roomController.OnJoinedRoom += JoinedRoomEventHandler;
        }

        public void Update()
        {
            if(_isPlayersSpawned && _appStateManager.AppState == Enumerators.AppState.Game)
                foreach (var player in _players)
                    if (player.Health != null)
                        player.Health.LookAtCamera();
        }

        public Player GetLocalPlayer()
        {
            return _players.FirstOrDefault(player => player.GetPlayerData().IsLocalPlayer);
        }
        
        public Player GetPlayerByID(int id)
        {
            return _players.FirstOrDefault(player => player.GetPlayerData().PlayerID == id);
        }

        public void ResetPlayersList()
        {
            _players = new List<Player>();
        }

        public List<string> GetCharacterPaths()
        {
            return _characterPrefabPaths;
        }

        public void ResetAll()
        {
            foreach (var player in _players) 
                player.Dispose();
            _players = new List<Player>();
        }

        public List<Player> GetPlayers()
        {
            return _players;
        }

        public List<GameConfig.CharacterData> GetCharactersData()
        {
            return _charactersData;
        }

        public GameConfig.CharacterData GetCharacterData(int characterID)
        {
            return _charactersData[characterID];
        }

        public void Dispose()
        {
            _gameEventsController.OnGridInit -= GridInitEventHandler;
            _gameEventsController.OnPlayerJoined -= PlayerJoinedEventHandler;
            _gameEventsController.OnPlayersUpdated -= PlayersUpdatedEventHandler;
            _gameEventsController.OnPlayerDisconnected -= PlayerDisconnectedEventHandler;
            _gameEventsController.OnGetGameConfig -= GetGameConfigEventHandler;
            _roomController.OnJoinedRoom -= JoinedRoomEventHandler;
            _appStateManager.OnAppStateChanged -= AppStateChangedEventHandler;
        }

        private void AddPlayer(PlayerData data)
        {
            _players.Add(new Player(data));
        }

        private void RemovePlayer(int id)
        {
            var player = _players.FirstOrDefault(p => p.GetPlayerData().PlayerID == id);
            _players.Remove(player);
        }

        private void SpawnPlayers(List<Player> players)
        {
            _players = players;
            var board = _gameplayManager.Board;
            _parentOfPlayers = GameObject.Find("GameField/ParentOfPlayers").transform;
            // var localPlayer = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/DwarfPlayer");
            // var botPlayer = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/BotPlayer");

            foreach (var player in players)
            {
                var prefabToSpawn = GetCharacterPrefab(player.GetPlayerData().CharacterID);
                var playerObject = MonoBehaviour.Instantiate(prefabToSpawn, _parentOfPlayers, false);
                player.Init(playerObject);

                var characterPoint = _characterPoints[player.GetPlayerData().PlayerID];
                var playerCell = board.GetCell(characterPoint.x, characterPoint.y);
                playerCell.SetUnit(player);
                playerObject.transform.position = playerCell.GetCellCenter();
            }

            OnPlayersSpawned?.Invoke();
            _isPlayersSpawned = true;
        }

        private GameObject GetCharacterPrefab(int characterID)
            => _loadObjectsManager.GetObjectByPath<GameObject>(_characterPrefabPaths[characterID]);

        private void ChooseLevel()
        {
            _parentOfLevels = GameObject.Find("LevelsParent").transform;
            GameObject level = _loadObjectsManager.GetObjectByPath<GameObject>($"Prefabs/Gameplay/Levels/Level_{_currentLevel}");
            MonoBehaviour.Instantiate(level, _parentOfLevels);
        }

        private void PlayerJoinedEventHandler(PlayerJoinedData data)
        {
            int characterId = ExtractCharacterIdFromParameters(data.config.parameters);

            AddPlayer(new PlayerData(data.id, characterId, _userProfileManager.IsLocalPlayer(data.config.userId), data.config.name));

            Debug.LogWarning($"PLAYER JOINED - ID: {data.id}, IsLocal: {_userProfileManager.IsLocalPlayer(data.config.userId)}, " +
                             $"UserID: {data.config.userId}, NetworkUPID: {_networkManager.UPID}, CharacterId: {characterId}");
        }

        private void PlayersUpdatedEventHandler(PlayersUpdatedData data)
        {
            List<Player> players = GetPlayers();
            foreach (var player in data.players)
            {
                if (players.Find(it => it.GetPlayerData().PlayerID == player.id) == null)
                {
                    int characterId = ExtractCharacterIdFromParameters(player.config.parameters);

                    AddPlayer(new PlayerData(player.id, characterId, _userProfileManager.IsLocalPlayer(player.config.userId), player.config.name));

                    Debug.LogWarning($"PLAYERS UPDATED - ID: {player.id}, IsLocal: {_userProfileManager.IsLocalPlayer(player.config.userId)}, " +
                                     $"UserID: {player.config.userId}, NetworkUPID: {_networkManager.UPID}");
                }
            }
        }

        private void GetGameConfigEventHandler(GameConfig data)
        {
            _characterPoints = data.grid.charactersPoints;
            _charactersData = data.characters;
            foreach (var characterData in _charactersData) 
                characterData.stamina = 100;
        }

        private void GridInitEventHandler(GridInitData data)
        {
            ChooseLevel();
            SpawnPlayers(GetPlayers());
        }

        private void AppStateChangedEventHandler(Enumerators.AppState state)
        {
            if (state == Enumerators.AppState.Main)
                _isPlayersSpawned = false;
        }

        private void JoinedRoomEventHandler(RoomInfo data)
        {
            _currentLevel = data.level;
        }

        private void PlayerDisconnectedEventHandler(PlayerDisconnectedData data)
        {
            RemovePlayer(data.id);
        }

        private int ExtractCharacterIdFromParameters(IDictionary<string, object> parameters)
        {
            if (parameters.TryGetValue("character", out var characterIdObject) 
                && characterIdObject is long longVal 
                && longVal <= int.MaxValue && longVal >= int.MinValue)
            {
                return (int)longVal;
            }

            return -1;
        }
    }
}