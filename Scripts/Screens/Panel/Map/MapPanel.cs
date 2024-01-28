using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GrandDevs.Tavern
{
    public class MapPanel
    {
        private readonly IUIManager _uiManager;
        private readonly IGameplayManager _gameplayManager;
        private readonly IUserProfileManager _userProfileManager;
        private readonly GameplayController _gameplayController;

        private GameObject _selfObject;
        private Transform _contentParent;

        private Map _map;
        private Button _backButton;
        
        private readonly string _tabsPanelPath = "Prefabs/UI/MapPageUI/MapPanel/MapPanel";

        public MapPanel(Transform contentParent)
        {
            _contentParent = contentParent;
            _userProfileManager = GameClient.Get<IUserProfileManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _uiManager = GameClient.Get<IUIManager>();

            Init(contentParent);
        }

        private void Init(Transform contentParent)
        {
            _selfObject = MonoBehaviour.Instantiate(Resources.Load<GameObject>(_tabsPanelPath), contentParent);
            _selfObject.transform.SetAsFirstSibling();

            _map = new Map(_selfObject.transform.Find("Map/Content").gameObject);
            _backButton = _selfObject.transform.Find("Button_Back").GetComponent<Button>();
            _backButton.onClick.AddListener(BackHandler);
        }

        private void BackHandler()
        {
            _uiManager.SetPage<MainPage>();
        }

        public void Dispose()
        {
            if(_selfObject!=null) 
                MonoBehaviour.Destroy(_selfObject);
        }

        public void Update()
        {
            _map.Update();
        }

    }
}