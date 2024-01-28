using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace GrandDevs.Tavern
{
    public class MenuProfilePanel
    {
        private readonly GameObject _selfObject;
        private readonly string _changeNamePanelPath = "Prefabs/UI/MainPageUI/MenuProfilePanel/MenuProfilePanel";
        private readonly string _changeNameViewPath = "Prefabs/UI/MainPageUI/MenuProfilePanel/MenuProfileView";

        private ChangeNameView _changeNameView;
        
        public MenuProfilePanel(Transform contentParent)
        {
            _selfObject = MonoBehaviour.Instantiate(Resources.Load<GameObject>(_changeNamePanelPath), contentParent);
            _selfObject.transform.SetAsFirstSibling();
            _changeNameView = new ChangeNameView(_changeNameViewPath, _selfObject.transform);
        }

        public void Dispose() => _changeNameView.Dispose();

        class ChangeNameView
        {
            private readonly IDataManager _dataManager;
            private readonly INetworkManager _networkManager;
            private GameObject _selfObject;
            private TMP_InputField _nameInputField;
            private TextMeshProUGUI _displayNameText;
            private string _playerName;
            private const string ValidNamePattern = @"^[a-zA-Z_-]+$";

            public ChangeNameView(string changeNamePath, Transform contentParent)
            {
                _dataManager = GameClient.Get<IDataManager>();
                _networkManager = GameClient.Get<INetworkManager>();
                var loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
                var objectByPath = loadObjectsManager.GetObjectByPath<GameObject>(changeNamePath);
                            
                _selfObject = MonoBehaviour.Instantiate(objectByPath, contentParent);
                // _displayNameText = _selfObject.transform.Find("Text_NameDisplay").GetComponent<TextMeshProUGUI>();
                _nameInputField = _selfObject.transform.Find("PlayerProfileVisual/InputField_Name").GetComponent<TMP_InputField>();
                _nameInputField.onEndEdit.AddListener(OnNameEntered);
                _nameInputField.onValidateInput += ValidateNameInput;

                SetPlayerName();
            }

            public void Dispose()
            {
                _nameInputField.onEndEdit.RemoveListener(OnNameEntered);
                _nameInputField.onValidateInput -= ValidateNameInput;
                MonoBehaviour.Destroy(_selfObject);
            }

            private char ValidateNameInput(string text, int charIndex, char addedChar)
            {
                if (!IsValidChar(addedChar))
                    return '\0';
                return addedChar;
            }
            
            private void SetPlayerName()
            {
                _playerName = _dataManager.CachedUserLocalData.name;
                _nameInputField.text = _playerName;
            }

            private void OnNameEntered(string newName)
            {
                if (string.IsNullOrEmpty(newName))
                {
                    Debug.LogError("Player name cannot be empty!");
                    return;
                }

                SaveAndDisplayPlayerName(newName);
            }

            private void SaveAndDisplayPlayerName(string playerName)
            {
                _dataManager.CachedUserLocalData.name = playerName;
                (_networkManager as NetworkManager).APIRequestHandler.SetUserProfile(_dataManager.CachedUserLocalData.token,playerName);
            }

            private bool IsValidChar(char c) => Regex.IsMatch(c.ToString(), ValidNamePattern);
            
        }
    }
}