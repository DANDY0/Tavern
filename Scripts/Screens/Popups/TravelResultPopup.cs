using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Enumerators = GrandDevs.Tavern.Common.Enumerators;
using Object = UnityEngine.Object;

namespace GrandDevs.Tavern
{
    public class TravelResultPopup: IUIPopup
    {
        public GameObject Self => _selfPopup;
        
        private GameObject _selfPopup;
        private IUIManager _uiManager;
        private ILoadObjectsManager _loadObjectsManager;
        private INetworkManager _networkManager;
        private IGameplayManager _gameplayManager;
        private ApiRequestHandler _apiRequestHandler;
        
        private TextMeshProUGUI _resultText;
        private Button _battleButton;
        private Button _skipBattleButton;
        private Button _claimItemButton;
        private Button _claimQuestButton;
        private Button _cancelQuestButton;

        private Dictionary<Enumerators.MapActions, string> _actionTexts = new Dictionary<Enumerators.MapActions, string>
        {
            {Enumerators.MapActions.Battle, "You are under attack"},
            {Enumerators.MapActions.Item, "You have got the item"},
            {Enumerators.MapActions.Quest, "You have got the quest"}
    };

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _networkManager = GameClient.Get<INetworkManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _apiRequestHandler = _networkManager.APIRequestHandler;
            
            _selfPopup = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/TravelResultPopup"));
            _selfPopup.transform.SetParent(_uiManager.Canvas.transform, false);
            _selfPopup.name = GetType().Name;
            
            _resultText = _selfPopup.transform.Find("Container/Text_Result").GetComponent<TextMeshProUGUI>();
            _battleButton = _selfPopup.transform.Find("Container/Button_Battle").GetComponent<Button>();
            _skipBattleButton = _selfPopup.transform.Find("Container/Button_SkipBattle").GetComponent<Button>();
            _claimItemButton = _selfPopup.transform.Find("Container/Button_ClaimItem").GetComponent<Button>();
            _claimQuestButton = _selfPopup.transform.Find("Container/Button_ClaimQuest").GetComponent<Button>();
            _cancelQuestButton = _selfPopup.transform.Find("Container/Button_CancelQuest").GetComponent<Button>();
            
            _battleButton.onClick.AddListener(BattleHandler);
            _claimItemButton.onClick.AddListener(ClaimItemHandler);
            _claimQuestButton.onClick.AddListener(ClaimQuestHandler);
            _cancelQuestButton.onClick.AddListener(CancelQuestHandler);
            
            Hide();
        }

        public void Hide()
        {
            _uiManager.FadeScreen(_selfPopup, false, () =>
            {
                _selfPopup.SetActive(false);
            });

            OffAllButtons();
        }

        public void Show(object data)
        {
            Show();
        }

        public void Show()
        {
            _selfPopup.SetActive(true);
            _uiManager.FadeScreen(_selfPopup, true);
        }

        public void Update()
        {
        }

        public void Dispose()
        {
        }

        public void SetMainPriority()
        {
        }

        private void BattleHandler()
        {
            _uiManager.HidePopup<TravelResultPopup>();
            //start battle
        }

        private async void SkipBattleHandler(int battleID)
        {
            _battleButton.onClick.RemoveListener(()=> SkipBattleHandler(battleID));
            _skipBattleButton.onClick.RemoveListener(()=> SkipBattleHandler(battleID));
            _uiManager.HidePopup<TravelResultPopup>();
            await _apiRequestHandler.PassBattleAsync(battleID, true);
        }

        private async void ClaimItemHandler()
        {
            _uiManager.HidePopup<TravelResultPopup>();
            await _apiRequestHandler.GetUserInventoryAsync();
        }

        private void CancelQuestHandler()
        {
            _uiManager.HidePopup<TravelResultPopup>();
        }

        private void ClaimQuestHandler()
        {
            _uiManager.HidePopup<TravelResultPopup>();
            //claim quest logic
        }

        private void OffAllButtons()
        {
            _battleButton.gameObject.SetActive(false);
            _skipBattleButton.gameObject.SetActive(false);
            _claimItemButton.gameObject.SetActive(false);
            _claimQuestButton.gameObject.SetActive(false);
            _cancelQuestButton.gameObject.SetActive(false);
        }

        public void ShowButtons(Enumerators.MapActions mapActionType, string actionID)
        {
            _resultText.text = _actionTexts[mapActionType];
            switch (mapActionType)
            {
                case Enumerators.MapActions.Battle:
                    _battleButton.gameObject.SetActive(true);
                    _skipBattleButton.gameObject.SetActive(true);
                    _skipBattleButton.onClick.AddListener(()=>SkipBattleHandler(int.Parse(actionID)));
                    
                    //temporary, then will start the battle
                    _battleButton.onClick.AddListener(()=>SkipBattleHandler(int.Parse(actionID)));

                    break;
                case Enumerators.MapActions.Item:
                    _claimItemButton.gameObject.SetActive(true);
                    break;
                case Enumerators.MapActions.Quest:
                    _claimQuestButton.gameObject.SetActive(true);
                    _cancelQuestButton.gameObject.SetActive(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mapActionType), mapActionType, null);
            }   
        }
    }
}