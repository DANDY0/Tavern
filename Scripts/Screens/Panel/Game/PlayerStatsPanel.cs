using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GrandDevs.Networking;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Enumerators = GrandDevs.Tavern.Common.Enumerators;
using Object = UnityEngine.Object;

namespace GrandDevs.Tavern
{
    public class PlayerStatsPanel
    {
        private readonly ILoadObjectsManager _loadObjectsManager;
        private readonly GameplayController _gameplayController;
        private readonly BoardInputController _boardInputController;
        private readonly GameObject _selfObject;
        private readonly Transform _contentParent;
        private readonly string _playerStatsPanelPath = "Prefabs/UI/GamePageUI/PlayerStatsPanel/PlayerStatsPanel";

        private List<Player> _players = new List<Player>();
        private PlayerStatsView _playerStatsView;

        public PlayerStatsPanel(Transform contentParent)
        {
            _contentParent = contentParent;
            var gameplayManager = GameClient.Get<IGameplayManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

            _gameplayController = gameplayManager.GetController<GameplayController>();
            _boardInputController = gameplayManager.GetController<BoardInputController>();
            _boardInputController.OnShowPlayerStats += ShowPlayerStatsEventHandler;
            _gameplayController.OnPlayersSpawned += OnPlayersSpawned;

            _selfObject = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>(_playerStatsPanelPath),
                _contentParent);
            _playerStatsView = new PlayerStatsView(_selfObject.transform);
        }

        public void Dispose()
        {
            _boardInputController.OnShowPlayerStats -= ShowPlayerStatsEventHandler;
            _gameplayController.OnPlayersSpawned -= OnPlayersSpawned;
            _playerStatsView.Dispose();
            _playerStatsView = null;
            if (_selfObject != null)
                MonoBehaviour.Destroy(_selfObject);
        }

        private void ShowPlayerStatsEventHandler(Player player)
        {
            _playerStatsView.ShowPlayerStats(player);
        }

        private void OnPlayersSpawned()
        {
            _players = _gameplayController.GetPlayers();
            // _playerStatsView = new PlayerStatsView(_selfObject.transform, _loadObjectsManager);
        }

        class PlayerStatsView
        {
            private readonly IGameplayManager _gameplayManager;
            private readonly ILoadObjectsManager _loadObjectsManager;
            private readonly GameplayController _gameplayController;
            private readonly GameObject _selfObject;
            private readonly CanvasGroup _canvasGroup;
            private HealthDefenseBarView _healthDefenseBarView;
            private PlayerDataView _playerDataView;
            private Button _closeButton;
            private Tween _fadeTween;
            private float _fadeDuration = .3f;

            public PlayerStatsView(Transform contentParent)
            {
                _gameplayManager = GameClient.Get<IGameplayManager>();
                _gameplayController = _gameplayManager.GetController<GameplayController>();
                _gameplayController.OnPlayersSpawned += PlayersSpawnedEventHandler;
                _gameplayManager.GameplayEndedEvent += GameplayEndedEvent;
                _selfObject = contentParent.Find("PlayerStatsView").gameObject;
                _canvasGroup = _selfObject.GetComponent<CanvasGroup>();
                _closeButton = _selfObject.transform.Find("Button_Close").GetComponent<Button>();
                _healthDefenseBarView = new HealthDefenseBarView(_selfObject.transform.Find("HpDefBarView").gameObject);
                _playerDataView = new PlayerDataView(_selfObject.transform.Find("PlayerDataView").gameObject);
                _closeButton.onClick.AddListener(() => ShowPanel(false));
                ShowPanel(false);
            }

            private void GameplayEndedEvent() => ShowPanel(false);

            private void PlayersSpawnedEventHandler() => ShowPanel(false);

            public void Dispose()
            {
                _gameplayController.OnPlayersSpawned -= PlayersSpawnedEventHandler;
                _gameplayManager.GameplayEndedEvent -= GameplayEndedEvent;
                _closeButton.onClick.RemoveListener(() => ShowPanel(false));
                _healthDefenseBarView.Dispose();
                _healthDefenseBarView = null;
                _playerDataView.Dispose();
                _playerDataView = null;
                if (_fadeTween != null)
                {
                    _fadeTween.Kill();
                    _fadeTween = null;
                }
            }

            public void ShowPlayerStats(Player player)
            {
                _playerDataView.SetPlayerData(player.GetPlayerData());
                _healthDefenseBarView.SetMaxValues(player.GetPlayerData().MaxHealth,
                    player.GetPlayerData().MaxHealth / 2);
                _healthDefenseBarView.SetPlayerHealthValues(player.Health);
                Debug.Log($"ShowStats player: {player.GetPlayerData().PlayerID}");
                ShowPanel(true);
            }

            private void ShowPanel(bool state)
            {
                if (_fadeTween != null && _fadeTween.IsActive() && !_fadeTween.IsComplete())
                    _fadeTween.Kill();

                float targetAlpha = state ? 1 : 0;
                _fadeTween =
                    DOTween.To(() => _canvasGroup.alpha, x
                        => _canvasGroup.alpha = x, targetAlpha, _fadeDuration);
            }
        }

        class PlayerDataView
        {
            private readonly GameObject _selfObject;
            private readonly IGameplayManager _gameplayManager;
            private readonly ILoadObjectsManager _loadObjectsManager;
            private readonly GameplayController _gameplayController;
            private readonly GameEventController _gameEventsController;

            private List<GameConfig.CharacterData> _characterData;
            private SkillsView _skillsView;
            private SkillsVisualData _skillsVisualData;
            private TextMeshProUGUI _playerName;
            private TextMeshProUGUI _ultimateName;
            private Image _playerAva;
            private Image _movePattern;
            private Image _ultimateImage;
            private int _characterID;
            private StatsContainer _statsContainer;

            public PlayerDataView(GameObject selfObject)
            {
                _selfObject = selfObject;
                _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
                _gameplayManager = GameClient.Get<IGameplayManager>();
                _gameEventsController = _gameplayManager.GetController<GameEventController>();
                
                _gameEventsController.OnGetGameConfig += OnGetGameConfig;
                _playerAva = _selfObject.transform.Find("Image_Ava").GetComponent<Image>();
                _ultimateImage = _selfObject.transform.Find("SkillsView/Ultimate/Image_UltIcon").GetComponent<Image>();
                _ultimateName = _selfObject.transform.Find("SkillsView/Ultimate/Text_UltimateName").GetComponent<TextMeshProUGUI>();
                _playerName = _selfObject.transform.Find("Text_PlayerName").GetComponent<TextMeshProUGUI>();
                _movePattern = _selfObject.transform.Find("MovePattern/Image_Pattern").GetComponent<Image>();
                _skillsVisualData = _loadObjectsManager.GetObjectByPath<SkillsVisualData>("ScriptableObjects/SkillsVisualData");
                _skillsView = new SkillsView(_selfObject.transform.Find("SkillsView").gameObject, _skillsVisualData);
                _statsContainer = new StatsContainer(_selfObject.transform.Find("StatsContainer").gameObject);

            }

            private void OnGetGameConfig(GameConfig data)
            {
                _characterData = _gameplayManager.GameConfig.characters;
                _skillsView.SetCharacterData(_characterData);
            }

            public void SetPlayerData(PlayerData playerData)
            {
                _characterID = playerData.CharacterID;
                _skillsView.SetCharacterID(_characterID);
                var characterVisualData = _skillsVisualData.CharacterVisualDatas[_characterID];
                var actionInfo = _characterData[_characterID].actions
                    .FirstOrDefault(a => a.action == Enumerators.CellType.AttackRegular.ToString());

                _playerAva.sprite = characterVisualData.Avatar;
                _playerName.text = playerData.Name;
                _movePattern.sprite = _skillsVisualData.MovePatterns[_characterID];
                _ultimateName.text = $"{characterVisualData.UltimateData.Name}";
                _ultimateImage.sprite = GetSpriteFromList(playerData.CharacterID);
                _statsContainer.SetStatsData(playerData, actionInfo);
            }

            public void Dispose()
            {
                _skillsVisualData = null;
                _skillsView.Dispose();
                _skillsView = null;
            }

            private Sprite GetSpriteFromList(int characterID)
            {
                return _skillsVisualData.CharacterVisualDatas[characterID].UltimateData.UltimateIcon;
            }

            class StatsContainer
            {
                private readonly GameObject _selfObject;
                private TextMeshProUGUI _attackPhysic;
                private TextMeshProUGUI _attackMagic;
                private TextMeshProUGUI _armour;
                private TextMeshProUGUI _magicArmour;
                private TextMeshProUGUI _crit;
                private TextMeshProUGUI _initiative;
                private TextMeshProUGUI _range;

                public StatsContainer(GameObject selfObject)
                {
                    _selfObject = selfObject;
                    _initiative = _selfObject.transform.Find("Initiative/Text_Value").GetComponent<TextMeshProUGUI>();
                    _range = _selfObject.transform.Find("Range/Text_Value").GetComponent<TextMeshProUGUI>();
                }

                public void SetStatsData(PlayerData playerData, GameConfig.ActionInfo actionInfo)
                {
                    _initiative.text = $"{playerData.Initiative}";
                    _range.text = $"{actionInfo!.range}";
                }
            }
        }

        class SkillsView
        {
            private readonly GameObject _selfObject;
            private readonly SkillsVisualData _skillsVisualData;

            private Dictionary<Enumerators.CellType, EventTrigger> _ultimateTrigger =
                new Dictionary<Enumerators.CellType, EventTrigger>();

            private Dictionary<Enumerators.SkillNames, EventTrigger> _skillTriggers =
                new Dictionary<Enumerators.SkillNames, EventTrigger>();

            private GameObject _hoverObject;
            private EventTrigger _enteredTrigger;
            private SkillsInfoBlockView _skillsInfoBlock;
            private int _characterID;
            public event Action<int, Enumerators.CellType> OnUltimateHovered;
            public event Action<int, Enumerators.SkillNames> OnSkillHovered;
            public event Action OnHoverExit;
            private bool _isHovering = false;

            public SkillsView(GameObject selfObject, SkillsVisualData skillsVisualData)
            {
                _selfObject = selfObject;
                _skillsVisualData = skillsVisualData;
                var skillBlockObj = _selfObject.transform.parent.parent.Find("SkillsInfoBlockView").gameObject;
                _skillsInfoBlock = new SkillsInfoBlockView(skillBlockObj, this, _skillsVisualData);
                Initialize();
                AddUltimateSkill();
                AddCommonSkillTriggers();
            }

            public void SetCharacterID(int characterID) => _characterID = characterID;

            public void SetCharacterData(List<GameConfig.CharacterData> characterData) => _skillsInfoBlock.SetCharactersData(characterData);

            public void Dispose()
            {
                OnSkillHovered = null;
                _skillsInfoBlock.Dispose();
                _skillsInfoBlock = null;
                RemoveListeners();
            }

            private void Initialize()
            {
                AddEventTrigger(_selfObject, (eventData) => { HandlePointerEnter((PointerEventData)eventData); },
                    EventTriggerType.PointerEnter);
                AddEventTrigger(_selfObject, (eventData) => { HandlePointerExit((PointerEventData)eventData); },
                    EventTriggerType.PointerExit);
            }

            void HandlePointerEnter(PointerEventData eventData)
            {
                _isHovering = true;
                var enteredTrigger = eventData.pointerEnter != null
                    ? eventData.pointerEnter.GetComponent<EventTrigger>()
                    : null;
                if (IsSkillHover(enteredTrigger))
                {
                    var skillType = _skillTriggers.FirstOrDefault(x => x.Value == enteredTrigger).Key;
                    OnSkillHovered?.Invoke(_characterID, skillType);
                    _hoverObject = eventData.pointerEnter;
                    // Debug.LogError("ON SKILL HOVERED");
                }
                else if (IsUltimateHover(enteredTrigger))
                {
                    var skillType = _ultimateTrigger.FirstOrDefault(x => x.Value == enteredTrigger).Key;
                    OnUltimateHovered?.Invoke(_characterID, skillType);
                    _hoverObject = eventData.pointerEnter;
                    // Debug.LogError("ON ULTIMATE HOVERED");
                }
                else
                    OnHoverExit?.Invoke();
            }

            private bool IsUltimateHover(EventTrigger enteredTrigger) =>
                enteredTrigger != null && _ultimateTrigger.ContainsValue(enteredTrigger);

            private bool IsSkillHover(EventTrigger enteredTrigger) =>
                enteredTrigger != null && _skillTriggers.ContainsValue(enteredTrigger);

            void HandlePointerExit(PointerEventData eventData)
            {
                if (_isHovering)
                {
                    _isHovering = false;
                    OnHoverExit?.Invoke();
                }
            }

            void AddUltimateSkill()
            {
                var ultimateSkill = _selfObject.transform.Find("Ultimate");
                GameObject ultimateObj = ultimateSkill.gameObject;
                var eventTrigger = ultimateObj.AddComponent<EventTrigger>();
                _ultimateTrigger.Add(Enumerators.CellType.Action, eventTrigger);
                // AddEventTrigger(ultimateObj, (eventData) => { HandlePointerExit((PointerEventData)eventData); }, EventTriggerType.PointerExit);
                AddEventTrigger(ultimateObj, (eventData) => { HandlePointerEnter((PointerEventData)eventData); },
                    EventTriggerType.PointerEnter);
            }

            void AddCommonSkillTriggers()
            {
                var commonSkillsParent = _selfObject.transform.Find("Skills");
                foreach (Transform child in commonSkillsParent)
                {
                    if (IsNameInEnum(child.transform.name))
                    {
                        GameObject childObj = child.gameObject;
                        var cellType =
                            (Enumerators.SkillNames)Enum.Parse(typeof(Enumerators.SkillNames), child.transform.name);
                        var eventTrigger = childObj.AddComponent<EventTrigger>();
                        _skillTriggers.Add(cellType, eventTrigger);
                        // AddEventTrigger(childObj, (eventData) => { HandlePointerExit((PointerEventData)eventData); }, EventTriggerType.PointerExit);
                        AddEventTrigger(childObj, (eventData) => { HandlePointerEnter((PointerEventData)eventData); },
                            EventTriggerType.PointerEnter);
                    }
                }
            }

            void AddEventTrigger(GameObject obj, UnityEngine.Events.UnityAction<BaseEventData> action,
                EventTriggerType eventTriggerType)
            {
                var eventTrigger = obj.GetComponent<EventTrigger>() ?? obj.AddComponent<EventTrigger>();
                eventTrigger.triggers = new List<EventTrigger.Entry>();

                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = eventTriggerType;
                entry.callback.AddListener(action);
                eventTrigger.triggers.Add(entry);
            }


            private void RemoveListeners()
            {
                foreach (var pair in _skillTriggers)
                {
                    if (pair.Value != null)
                    {
                        pair.Value.triggers.Clear();
                        UnityEngine.Object.Destroy(pair.Value);
                    }
                }

                _skillTriggers.Clear();
            }

            private bool IsNameInEnum(string name)
            {
                foreach (string enumName in Enum.GetNames(typeof(Enumerators.SkillNames)))
                    if (enumName == name)
                        return true;
                return false;
            }
        }

        class SkillsInfoBlockView
        {
            private readonly GameObject _selfObject;
            private readonly IGameplayManager _gameplayManager;
            private readonly GameplayController _gameplayController;
            private readonly SkillsView _skillsView;
            private readonly SkillsVisualData _skillsVisualData;
            private List<GameConfig.CharacterData> _characterData;
            private Image _skillIcon;
            private TextMeshProUGUI _skillName;
            private TextMeshProUGUI _skillDescription;
            private TextMeshProUGUI _rangeText;
            private TextMeshProUGUI _valueText;
            private CanvasGroup _canvasGroup;
            private Tween _fadeTween;
            private float _fadeDuration = 0.1f;

            public SkillsInfoBlockView(GameObject selfObject, SkillsView skillsView, SkillsVisualData skillsVisualData)
            {
                _selfObject = selfObject;
                _gameplayManager = GameClient.Get<IGameplayManager>();
                _gameplayController = _gameplayManager.GetController<GameplayController>();
                _skillsView = skillsView;
                _skillsVisualData = skillsVisualData;
                _canvasGroup = _selfObject.AddComponent<CanvasGroup>();
                _skillIcon = _selfObject.transform.Find("Image_Skill").GetComponent<Image>();
                _skillName = _selfObject.transform.Find("Text_SkillName").GetComponent<TextMeshProUGUI>();
                _skillDescription = _selfObject.transform.Find("Text_Description").GetComponent<TextMeshProUGUI>();
                _rangeText = _selfObject.transform.Find("Stats/Text_Range").GetComponent<TextMeshProUGUI>();
                _valueText = _selfObject.transform.Find("Stats/Text_Value").GetComponent<TextMeshProUGUI>();

                _gameplayController.OnPlayersSpawned += PlayersSpawnedEventHandler;
                _gameplayManager.GameplayEndedEvent += GameplayEndedEvent;
                _skillsView.OnUltimateHovered += UltimateHoveredEventHandler;
                _skillsView.OnSkillHovered += SkillHoveredEventHandler;
                _skillsView.OnHoverExit += HoverExitEventHandler;
                _canvasGroup.alpha = 0;
            }

            public void SetCharactersData(List<GameConfig.CharacterData> characterData) => _characterData = characterData;

            public void Dispose()
            {
                _gameplayController.OnPlayersSpawned -= PlayersSpawnedEventHandler;
                _skillsView.OnUltimateHovered -= UltimateHoveredEventHandler;
                _skillsView.OnSkillHovered -= SkillHoveredEventHandler;
                _skillsView.OnHoverExit -= HoverExitEventHandler;
                if (_fadeTween != null)
                {
                    _fadeTween.Kill();
                    _fadeTween = null;
                }
            }

            private void PlayersSpawnedEventHandler() => _canvasGroup.alpha = 0;
            
            private void HoverExitEventHandler() => ShowPanel(false);

            private void GameplayEndedEvent()
            {
                if (_fadeTween != null)
                {
                    _fadeTween.Kill();
                    _fadeTween = null;
                }
            }

            private void SkillHoveredEventHandler(int characterID, Enumerators.SkillNames skillType)
            {
                SetPanelData(characterID, skillType);
                ShowPanel(true);
            }

            private void UltimateHoveredEventHandler(int characterID, Enumerators.CellType skillType)
            {
                SetPanelData(characterID, skillType);
                ShowPanel(true);
            }


            private void SetPanelData(int characterID, Enumerators.SkillNames skillType)
            {
                // _skillName.text = skillType.ToString();

                var skillData = _skillsVisualData.SkillsData.FirstOrDefault(s => s.CellType == skillType);
                _skillIcon.sprite = skillData.SkillIcon;
                _skillName.text = skillData.Name;
                _skillDescription.text = skillData.Description;


                var actionsInfo = _characterData[characterID].actions;
                var action = actionsInfo.FirstOrDefault(a =>
                    a.cell.Equals(skillType.ToString(), StringComparison.OrdinalIgnoreCase));

                if (action != null)
                {
                    _rangeText.text = $"Range: {action.range}";
                    _valueText.text = $"Value: {action.value}";
                }
                else
                {
                    _rangeText.text = "Range: N/A";
                    _valueText.text = "Value: N/A";
                }
            }

            private void SetPanelData(int characterID, Enumerators.CellType ultimateType)
            {
                if (ultimateType == Enumerators.CellType.Action)
                {
                    var skillData = _skillsVisualData.CharacterVisualDatas[characterID].UltimateData;
                    _skillIcon.sprite = skillData.UltimateIcon;
                    _skillName.text = skillData.Name;
                    _skillDescription.text = skillData.Description;
                }

                var actionsInfo = _characterData[characterID].actions;
                var action = actionsInfo.FirstOrDefault(a =>
                    a.cell.Equals(ultimateType.ToString(), StringComparison.OrdinalIgnoreCase));

                if (action != null)
                {
                    _rangeText.text = $"Range: {action.range}";
                    _valueText.text = $"Value: {action.value}";
                }
                else
                {
                    _rangeText.text = "Range: N/A";
                    _valueText.text = "Value: N/A";
                }
            }

            private void ShowPanel(bool state)
            {
                if (_fadeTween != null && _fadeTween.IsActive() && !_fadeTween.IsComplete())
                    _fadeTween.Kill();

                float targetAlpha = state ? 1 : 0;
                _fadeTween =
                    DOTween.To(() => _canvasGroup.alpha, x
                        => _canvasGroup.alpha = x, targetAlpha, _fadeDuration);
            }
        }

        class HealthDefenseBarView
        {
            private readonly GameObject _selfObject;
            private readonly Image _healthBarImage;
            private readonly Image _defenseBarImage;
            private TextMeshProUGUI _healthText;
            private TextMeshProUGUI _defenseText;
            private HealthBar _currentPlayerHealth;
            private int _maxHealthValue;
            private int _maxDefenseValue;

            public HealthDefenseBarView(GameObject selfObject)
            {
                _selfObject = selfObject;
                _healthBarImage = _selfObject.transform.Find("Image_HealthBack/Image_Health").GetComponent<Image>();
                _defenseBarImage = _selfObject.transform.Find("Image_DefenseBack/Image_Defense").GetComponent<Image>();
                _healthText = _selfObject.transform.Find("Image_HealthBack/Text_Health")
                    .GetComponent<TextMeshProUGUI>();
                _defenseText = _selfObject.transform.Find("Image_DefenseBack/Text_Defense")
                    .GetComponent<TextMeshProUGUI>();
            }

            public void SetPlayerHealthValues(HealthBar healthBar)
            {
                if (healthBar == _currentPlayerHealth)
                    return;

                var healthPercentage = healthBar.GetCurrentHealthPercentage();
                var defensePercentage = healthBar.GetCurrentDefensePercentage();
                UpdateHealthBar(healthPercentage);
                UpdateDefenseBar(defensePercentage);
                Subscribe(healthBar);
            }

            public void Dispose()
            {
            }

            public void SetMaxValues(int maxHealth, int maxDefense)
            {
                _maxHealthValue = maxHealth;
                _maxDefenseValue = maxDefense;
            }

            private void UpdateHealthBar(float healthPercentage)
            {
                // Debug.Log($"STATS HealthChanged : {healthPercentage}");
                _healthBarImage.fillAmount = healthPercentage;
                _healthText.text = $"{Mathf.Round(healthPercentage * _maxHealthValue)} / {_maxHealthValue}";
            }

            private void UpdateDefenseBar(float defensePercentage)
            {
                // Debug.Log($"STATS DefenseChanged : {defensePercentage}");
                _defenseBarImage.fillAmount = defensePercentage;
                _defenseText.text = $"{Mathf.Round(defensePercentage * _maxDefenseValue)} / {_maxDefenseValue}";
            }

            private void Subscribe(HealthBar healthBar)
            {
                if (_currentPlayerHealth != null)
                {
                    _currentPlayerHealth.OnHealthChanged -= UpdateHealthBar;
                    _currentPlayerHealth.OnDefenseChanged -= UpdateDefenseBar;
                }

                _currentPlayerHealth = healthBar;
                _currentPlayerHealth.OnHealthChanged += UpdateHealthBar;
                _currentPlayerHealth.OnDefenseChanged += UpdateDefenseBar;
            }
        }
    }
}