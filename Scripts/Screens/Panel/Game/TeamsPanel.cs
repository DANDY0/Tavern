using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GrandDevs.Tavern
{
    public class TeamsPanel
    {
        private readonly IGameplayManager _gameplayManager;
        private readonly GameplayController _gameplayController;
        private readonly Transform _contentParent;
        private readonly GameObject _selfObject;
        private readonly Transform _playerPlatesParent;

        private readonly string _pathToTeamsPanel = "Prefabs/UI/GamePageUI/TeamsPanel/TeamsPanel";
        private readonly string _pathToVersusElement = "Prefabs/UI/GamePageUI/TeamsPanel/VsVisual";
        private readonly string _pathToAllyPrefab = "Prefabs/UI/GamePageUI/TeamsPanel/PlayerPlateAlly";
        private readonly string _pathToEnemyPrefab = "Prefabs/UI/GamePageUI/TeamsPanel/PlayerPlateEnemy";

        private List<TeamViewItem> _items = new List<TeamViewItem>();

        public TeamsPanel(Transform contentParent)
        {
            _contentParent = contentParent;
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _gameplayController = _gameplayManager.GetController<GameplayController>();
            _gameplayController.OnPlayersSpawned += PlayersSpawnedEventHandler;
            _gameplayManager.GameplayEndedEvent += GameplayEndedEvent;

            _selfObject = Object.Instantiate(Resources.Load<GameObject>(_pathToTeamsPanel), _contentParent);
            _playerPlatesParent = _selfObject.transform.Find("Content/Teams/PlayersPlates");
        }

        private void GameplayEndedEvent()
        {
            foreach (var item in _items) 
                item.Dispose();
        }

        private void PlayersSpawnedEventHandler()
        {
            var players = _gameplayController.GetPlayers();
            var allyTeam = players
                .Where(player => player.GetPlayerData().IsLocalPlayer).ToList();
            var enemyTeam = players
                .Where(player => !player.GetPlayerData().IsLocalPlayer).ToList();
            
            FillTeams(allyTeam, enemyTeam);
            Debug.Log("PLAYERSSPAWNED");
        }

        public void Dispose()
        {
            _gameplayController.OnPlayersSpawned -= PlayersSpawnedEventHandler;
            if (_selfObject != null) 
                MonoBehaviour.Destroy(_selfObject);
        }

        private void FillTeams(List<Player> allyTeam, List<Player> enemyTeam)
        {
            foreach (var player in allyTeam)
                _items.Add(new TeamMemberItem(player, _pathToAllyPrefab, _playerPlatesParent));
            
            _items.Add(new TeamViewItem(_pathToVersusElement, _playerPlatesParent));

            foreach (var player in enemyTeam)
                _items.Add(new TeamMemberItem(player, _pathToEnemyPrefab, _playerPlatesParent));
        }

        private class TeamViewItem
        {
            protected GameObject _selfObject;

            public TeamViewItem(string prefabPath, Transform teamsParent)
            {
                _selfObject = Object.Instantiate(Resources.Load<GameObject>(prefabPath), teamsParent);
            }

            public void Dispose() => 
                MonoBehaviour.Destroy(_selfObject);
        }

        private class TeamMemberItem : TeamViewItem
        {
            private readonly Player _player;
            private readonly HealthDefenseBarView _healthDefenseBarView;
            private readonly PlayerDataView _playerDataView;

            public TeamMemberItem(Player player, string memberPath, Transform teamsParent) : base(memberPath, teamsParent)
            {
                _player = player;
                var healthBarGameObject = _selfObject.transform.Find("HealthDefenseBar").gameObject;
                _healthDefenseBarView = new HealthDefenseBarView(healthBarGameObject);
                var playerDataGameObject = _selfObject.transform.Find("PlayerDataVisual").gameObject;
                _playerDataView = new PlayerDataView(playerDataGameObject);
                _playerDataView.Init(_player.GetPlayerData());
                _player.Health.OnHealthChanged -= HealthChangedEventHandler;
                _player.Health.OnDefenseChanged -= DefenseChangedEventHandler;
                _player.Health.OnHealthChanged += HealthChangedEventHandler;
                _player.Health.OnDefenseChanged += DefenseChangedEventHandler;
                SetMaxValues();
                HealthChangedEventHandler(1);
                DefenseChangedEventHandler(0);
            }

            private void SetMaxValues() => _healthDefenseBarView.SetMaxValues(_player.GetPlayerData().MaxHealth,_player.GetPlayerData().MaxHealth/2);
            private void HealthChangedEventHandler(float newValue) => _healthDefenseBarView.UpdateHealthBar(newValue);
            private void DefenseChangedEventHandler(float newValue) => _healthDefenseBarView.UpdateDefenseBar(newValue);
        }
        
        class HealthDefenseBarView
        {
            private readonly GameObject _selfObject;
            private Image _healthBarImage;
            private Image _defenseBarImage;
            private TextMeshProUGUI _healthText;
            private TextMeshProUGUI _defenseText;

            private readonly Vector3 _originalHpScale;
            private readonly Vector3 _originalDefScale;
            private int _maxHealthValue;
            private int _maxDefenseValue;
            
            public HealthDefenseBarView(GameObject selfObject)
            {
                _selfObject = selfObject;
                _healthBarImage = _selfObject.transform.Find("Image_HealthBarBack/Image_HealthBar").GetComponent<Image>();
                _defenseBarImage = _selfObject.transform.Find("Image_DefenseBarBack/Image_DefenseBar").GetComponent<Image>();
                _healthText = _selfObject.transform.Find("Image_HealthBarBack/Text_HealthValue").GetComponent<TextMeshProUGUI>();
                _defenseText = _selfObject.transform.Find("Image_DefenseBarBack/Text_DefenseValue").GetComponent<TextMeshProUGUI>();

                _originalHpScale = _healthBarImage.rectTransform.localScale;
                _originalDefScale = _defenseBarImage.rectTransform.localScale;
            }
            
            public void UpdateHealthBar(float healthPercentage)
            {
                _healthBarImage.rectTransform.localScale = new Vector3(healthPercentage, _originalHpScale.y,_originalHpScale.z);
                _healthText.text = $"{Mathf.Round(healthPercentage * _maxHealthValue)} / {_maxHealthValue}";
            }

            public void UpdateDefenseBar(float defensePercentage)
            {
                _defenseBarImage.rectTransform.localScale = new Vector3(defensePercentage, _originalDefScale.y,_originalDefScale.z);
                _defenseText.text = $"{Mathf.Round(defensePercentage * _maxDefenseValue)} / {_maxDefenseValue}";
            }

            public void SetMaxValues(int maxHealth, int maxDefense)
            {
                _maxHealthValue = maxHealth;
                _maxDefenseValue = maxDefense;
            }
        } 
        
        class PlayerDataView
        {
            private readonly GameObject _selfObject;
            private TextMeshProUGUI _playerName;
            private Image _playerImage;
            private List<List<Image>> _collageImages;
            
            public PlayerDataView(GameObject selfObject)
            {
                _selfObject = selfObject;
                _playerName = _selfObject.transform.Find("Text_PlayerName").GetComponent<TextMeshProUGUI>();
                _playerImage = _selfObject.transform.Find("PlayerMask/Image_Player").GetComponent<Image>();
            }

            public void Init(PlayerData playerData)
            {
                _playerName.text =  GameClient.Get<IGameplayManager>().GetController<GameplayController>()
                    .GetPlayerByID(playerData.PlayerID).GetPlayerData().Name;

                var loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
                _playerImage.sprite = loadObjectsManager.GetObjectByPath<Sprite>($"Sprites/ava/{playerData.CharacterID}");
            }
        }

    }
}