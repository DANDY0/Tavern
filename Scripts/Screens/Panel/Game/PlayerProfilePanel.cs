using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrandDevs.Tavern
{
    public class PlayerProfilePanel
    {
        private readonly GameplayController _gameplayController;
        private readonly GameObject _selfObject;
        private readonly Transform _contentParent;
        private readonly string _playerProfilePanelPath = "Prefabs/UI/GamePageUI/PlayerProfilePanel/PlayerProfilePanel";
        private readonly string _playerProfileViewPath = "Prefabs/UI/GamePageUI/PlayerProfilePanel/PlayerProfileView";

        private PlayerProfileView _playerProfileView;

        public PlayerProfilePanel(Transform contentParent)
        {
            _contentParent = contentParent;
            _gameplayController = GameClient.Get<IGameplayManager>().GetController<GameplayController>();
            // _gameplayController.OnPlayersSpawned += OnPlayersSpawned;
            _selfObject = Object.Instantiate(Resources.Load<GameObject>(_playerProfilePanelPath), _contentParent);
            OnPlayersSpawned();
        }

        public void Dispose()
        {
            // _gameplayController.OnPlayersSpawned -= OnPlayersSpawned;
            _playerProfileView.Dispose();
            if (_selfObject != null) 
                MonoBehaviour.Destroy(_selfObject);
        }

        private void OnPlayersSpawned()
        {
            _playerProfileView = new PlayerProfileView(_selfObject.transform, _playerProfileViewPath);
        }

        class PlayerProfileView
        {
            private readonly GameplayController _gameplayController;
            private readonly GameObject _selfObject;
            private Player _localPlayer;
            private HealthDefenseBarView _healthDefenseBarView;
            private PlayerDataView _playerDataView;

            public PlayerProfileView(Transform contentParent, string playerProfilePath)
            {
                var loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
                var objectByPath = loadObjectsManager.GetObjectByPath<GameObject>(playerProfilePath);

                _selfObject = MonoBehaviour.Instantiate(objectByPath, contentParent);
                _gameplayController = GameClient.Get<IGameplayManager>().GetController<GameplayController>();
            
                var healthBarGameObject = _selfObject.transform.Find("HpDefBar").gameObject;
                var playerDataVisualObject = _selfObject.transform.Find("PlayerDataVisual").gameObject;
                _healthDefenseBarView = new HealthDefenseBarView(healthBarGameObject);
                _playerDataView = new PlayerDataView(playerDataVisualObject);

                _gameplayController.OnPlayersSpawned += OnPlayersSpawned;
            }

            private void OnPlayersSpawned()
            {
                _localPlayer = _gameplayController.GetLocalPlayer();
                _localPlayer.Health.OnHealthChanged += HealthChangedEventHandler;
                _localPlayer.Health.OnDefenseChanged += DefenseChangedEventHandler;
                _playerDataView.Init(_localPlayer.GetPlayerData().PlayerID);
                HealthChangedEventHandler(_localPlayer.GetPlayerData().MaxHealth);
                DefenseChangedEventHandler(_localPlayer.GetPlayerData().Defense);
            }

            private void HealthChangedEventHandler(float newValue)
            {
                _healthDefenseBarView.UpdateHealthBar(newValue);
            }

            private void DefenseChangedEventHandler(float newValue)
            {
                _healthDefenseBarView.UpdateDefenseBar(newValue);
            }

            public void Dispose()
            {
                _gameplayController.OnPlayersSpawned -= OnPlayersSpawned;
            }

        }

        class PlayerDataView
        {
            private readonly GameObject _selfObject;
            private TextMeshProUGUI _playerName;
            private Image _playerImage;
            
            public PlayerDataView(GameObject selfObject)
            {
                _selfObject = selfObject;
                _playerName = _selfObject.transform.Find("PlayerImageMask/PlayerName/Text_PlayerName").GetComponent<TextMeshProUGUI>();
                _playerImage = _selfObject.transform.Find("PlayerImageMask/Image_Player").GetComponent<Image>();
            }

            public void Init(int playerID)
            {
                // _playerName.text = $"Player {playerID+1}";
                _playerName.text =  GameClient.Get<IGameplayManager>().GetController<GameplayController>()
                    .GetPlayerByID(playerID).GetPlayerData().Name;
                var loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
                // _playerImage.sprite = loadObjectsManager.GetObjectByPath<Sprite>($"Sprites/ava/{playerID+1}");
            }
        }
        
        class HealthDefenseBarView
        {
            private readonly GameObject _selfObject;
            private readonly Image _healthBarImage;
            private readonly Image _defenseBarImage;

            public HealthDefenseBarView(GameObject selfObject)
            {
                _selfObject = selfObject;
                _healthBarImage = _selfObject.transform.Find("Image_Health").GetComponent<Image>();
                _defenseBarImage = _selfObject.transform.Find("Image_Defense").GetComponent<Image>();
            }

            public void UpdateHealthBar(float healthPercentage)
            {
                _healthBarImage.fillAmount = healthPercentage;
            }

            public void UpdateDefenseBar(float defensePercentage)
            {
                _defenseBarImage.fillAmount = defensePercentage;
            }

        } 
    }

}