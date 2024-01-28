using System;
using System.Collections.Generic;
using System.Linq;
using GrandDevs.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GrandDevs.Tavern
{
    public class TurnPlayersPanel
    {
        private readonly IGameplayManager _gameplayManager;
        private readonly GameplayController _gameplayController;
        private readonly RoundActionsController _roundActionController;
        private readonly RoundEventController _roundEventController;
        private readonly GameEventController _gameEventController;
        private readonly GameObject _selfObject;
        private readonly Transform _playersTurnParent;

        private readonly string _pathToPlayersTurnPanel = "Prefabs/UI/GamePageUI/PlayersTurnPanel/PlayersTurnPanel";
        private readonly string _pathToPlayerTurnItemAlly = "Prefabs/UI/GamePageUI/PlayersTurnPanel/PlayerTurnItemAlly";
        private readonly string _pathToPlayerTurnItemEnemy = "Prefabs/UI/GamePageUI/PlayersTurnPanel/PlayerTurnItemEnemy";
        private readonly string _pathToRightTurnVisual = "Prefabs/UI/GamePageUI/PlayersTurnPanel/RightSideTurnVisual";
        private readonly string _pathToLeftTurnVisual = "Prefabs/UI/GamePageUI/PlayersTurnPanel/LeftSideTurnVisual";

        private TextMeshProUGUI _playerTurnName;

        private List<TurnPanelItem> _items;
        private List<int> _originalTurnOrder;
        private List<int> _currentTurnOrder;
        private PlayerTurnItem _currentActivePlayer;

        private int _currentActiveItemIndex = 1;
        private bool _isFirstTurn = true;

        public TurnPlayersPanel(Transform contentParent)
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _gameplayController = _gameplayManager.GetController<GameplayController>();
            _roundActionController = _gameplayManager.GetController<RoundActionsController>();
            _roundEventController = _gameplayManager.GetController<RoundEventController>();
            _gameEventController = _gameplayManager.GetController<GameEventController>();
            _gameplayController.OnPlayersSpawned += PlayersSpawnedEventHandler;
            _gameplayManager.GameplayEndedEvent += GameplayEndedEvent;
            _roundActionController.OnCombatActionProcessed += CombatActionProcessedEventHandler;
            _roundEventController.OnRoundStarted += RoundStartedEventHandler;
            _roundEventController.OnRoundEnded += RoundEndedEventHandler;
            _gameEventController.OnGridInit += GridInitEventHandler;

            _selfObject = Object.Instantiate(Resources.Load<GameObject>(_pathToPlayersTurnPanel), contentParent);
            _playersTurnParent = _selfObject.transform.Find("ProfilesArea");
            _playerTurnName = _selfObject.transform.Find("Text_PlayerTurnName").GetComponent<TextMeshProUGUI>();
            _items = new List<TurnPanelItem>();
            _originalTurnOrder = new List<int>();
            _currentTurnOrder = new List<int>();
        }

        private void GameplayEndedEvent()
        {
            foreach (var item in _items) 
                item.Dispose();
            _items = new List<TurnPanelItem>();
        }

        public void Dispose()
        {
            _gameplayController.OnPlayersSpawned -= PlayersSpawnedEventHandler;
            _gameplayManager.GameplayEndedEvent -= GameplayEndedEvent;
            _roundActionController.OnCombatActionProcessed -= CombatActionProcessedEventHandler;
            _roundEventController.OnRoundStarted -= RoundStartedEventHandler;
            _roundEventController.OnRoundEnded -= RoundEndedEventHandler;
            _gameEventController.OnGridInit -= GridInitEventHandler;

            if (_selfObject != null) 
                Object.Destroy(_selfObject);

            _items.Clear();
            _originalTurnOrder?.Clear();
            _currentTurnOrder?.Clear();
        }

        private void GridInitEventHandler(GridInitData data)
        {
            if (data.order != null)
            {
                _originalTurnOrder = new List<int>(data.order);
                _currentTurnOrder = new List<int>(_originalTurnOrder);
            }
        }

        private void RoundStartedEventHandler(RoundStartedData data)
        {
            _playerTurnName.text = String.Empty;
            _currentTurnOrder = new List<int>(_originalTurnOrder);
            SortItemsByTurnOrder();
        }

        private void RoundEndedEventHandler()
        {
            foreach (var item in _items)
                if (item is PlayerTurnItem playerTurnItem)
                    playerTurnItem.PlayerTurnItemView.DeactivateItem();

            _isFirstTurn = true;
        }

        private void CombatActionProcessedEventHandler(int playerID)
        {
            // _playerTurnName.text = $"Turn player: {playerID + 1}";
            _playerTurnName.text =  _gameplayController.GetPlayerByID(playerID).GetPlayerData().Name;
            
            int currentPlayerOrderIndex = _currentTurnOrder.IndexOf(playerID);
            if (currentPlayerOrderIndex == -1)
            {
                Debug.LogError($"Player with ID {playerID} not found in turn order list.");
                return;
            }

            DeactivateCurrentPlayer();

            UpdatePlayerOrder(currentPlayerOrderIndex);

            ActivatePlayer(playerID);
        }

        private void DeactivateCurrentPlayer()
        {
            _currentActivePlayer?.PlayerTurnItemView.DeactivateItem();
            _currentActivePlayer = null;
        }

        private void ActivatePlayer(int playerID)
        {
            PlayerTurnItem currentPlayer = _items.FirstOrDefault(item =>
                item is PlayerTurnItem playerTurnItem && playerTurnItem.PlayerID == playerID) as PlayerTurnItem;
            if (currentPlayer != null)
            {
                currentPlayer.PlayerTurnItemView.ActivateItem();
                _currentActivePlayer = currentPlayer;
            }
        }

        private void UpdatePlayerOrder(int currentPlayerOrderIndex)
        {
            List<int> newOrder = _currentTurnOrder.Skip(currentPlayerOrderIndex).ToList();
            newOrder.AddRange(_currentTurnOrder.Take(currentPlayerOrderIndex));
            _currentTurnOrder = newOrder;

            SortItemsByTurnOrder();
        }

        private void SortItemsByTurnOrder()
        {
            List<TurnPanelItem> sortedItems = new List<TurnPanelItem> { _items[0] };

            foreach (int playerId in _currentTurnOrder)
            {
                TurnPanelItem matchingItem = _items.FirstOrDefault(item =>
                    item is PlayerTurnItem playerTurnItem && playerTurnItem.PlayerID == playerId);
                if (matchingItem != null)
                    sortedItems.Add(matchingItem);
            }

            sortedItems.Add(_items.Last());

            _items = sortedItems;

            for (int i = 0; i < _items.Count; i++) 
                _items[i].SelfObject.transform.SetSiblingIndex(i);
        }

        private void PlayersSpawnedEventHandler()
        {
            var players = _gameplayController.GetPlayers();
            var playersOrderedList = players.OrderBy(player => player.GetPlayerData().PlayerID).ToList();

            FillPanel(playersOrderedList);
        }

        private void FillPanel(List<Player> playersOrderedList)
        {
            _items.Add(new TurnPanelItem(_pathToLeftTurnVisual, _playersTurnParent));

            foreach (var player in playersOrderedList)
            {
                var playerItem = player.GetPlayerData().IsLocalPlayer
                    ? new PlayerTurnItem(player, _pathToPlayerTurnItemAlly, _playersTurnParent)
                    : new PlayerTurnItem(player, _pathToPlayerTurnItemEnemy,
                        _playersTurnParent);

                playerItem.PlayerTurnItemView.DeactivateItem();
                _items.Add(playerItem);
            }

            _items.Add(new TurnPanelItem(_pathToRightTurnVisual, _playersTurnParent));
        }
    }

    class TurnPanelItem
    {
        public readonly GameObject SelfObject;

        public TurnPanelItem(string prefabPath, Transform teamsParent)
        {
            SelfObject = Object.Instantiate(Resources.Load<GameObject>(prefabPath), teamsParent);
        }

        public virtual void Dispose() => 
            MonoBehaviour.Destroy(SelfObject);
    }

    class PlayerTurnItem : TurnPanelItem
    {
        private readonly Player _player;
        public readonly int PlayerID;
        public bool IsDead;
        public PlayerTurnItemView PlayerTurnItemView;


        public PlayerTurnItem(Player player, string memberPath, Transform teamsParent) : base(memberPath,
            teamsParent)
        {
            _player = player;
            var playerID = player.GetPlayerData().PlayerID;
            IsDead = false;
            PlayerID = playerID;
            PlayerTurnItemView = new PlayerTurnItemView(SelfObject.transform, player.GetPlayerData());
            _player.Health.OnPlayerDead += OnPlayerDead;
        }

        private void OnPlayerDead()
        {
            IsDead = true;
            PlayerTurnItemView.SetDead();
        }

        public override void Dispose()
        {
            PlayerTurnItemView = null;
            _player.Health.OnPlayerDead -= OnPlayerDead;
            base.Dispose();
        }
    }

    class PlayerTurnItemView
    {
        private readonly Transform _selfObject;
        private readonly Image _playerImage;
        private readonly Image _playerDeadImage;
        private readonly CanvasGroup _canvasGroup;
        private readonly float _activeAlphaValue = 1f;
        private readonly float _deactiveAlphaValue = .5f;
        
        private bool _isDead;

        public PlayerTurnItemView(Transform selfObject, PlayerData playerData)
        {
            _selfObject = selfObject;
            _canvasGroup = selfObject.transform.Find("Content").GetComponent<CanvasGroup>();
            _playerImage = selfObject.transform.Find("Content/PlayerCircle/Image_Player").GetComponent<Image>();
            _playerDeadImage = selfObject.transform.Find("Content/PlayerCircle/Image_DeadPlayer").GetComponent<Image>();

            var loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _playerImage.sprite = loadObjectsManager.GetObjectByPath<Sprite>($"Sprites/ava/{playerData.CharacterID}");
            _playerDeadImage.enabled = false;
        }

        public void ActivateItem()
        {
            if(_isDead)
                return;
            _canvasGroup.alpha = _activeAlphaValue;
        }

        public void DeactivateItem()
        {
            if(_isDead)
                return;
            _canvasGroup.alpha = _deactiveAlphaValue;
        }

        public void SetDead()
        {
            _canvasGroup.alpha = 1;
            _playerDeadImage.enabled = true;
            _isDead = true;
        }
    }
}