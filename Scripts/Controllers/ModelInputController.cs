using System;
using System.Collections.Generic;
using GrandDevs.Tavern.Common;
using UnityEngine;

namespace GrandDevs.Tavern
{
    public class ModelInputController: IController
    {
        private readonly IScenesManager _scenesManager;
        private readonly IUserProfileManager _userProfileManager;

        private Camera _camera;
        private Transform _modelsParent;

        private List<string> _characterPrefabPaths = new List<string>
        {
            { "Prefabs/Gameplay/RotateCharacterModels/Troll" },
            { "Prefabs/Gameplay/RotateCharacterModels/Bear" },
            { "Prefabs/Gameplay/RotateCharacterModels/Knight" },
            { "Prefabs/Gameplay/RotateCharacterModels/Demon" }
        };
        private List<RotateModel> _characterModels;
        private RotateModel _currentModel;

        private Vector3 _startMousePosition;
        private LayerMask _clickableLayer;

        private bool _isRotating = false;
        private bool _canHandleInput = true;
        private float _rotateSpeed = 20;

        public ModelInputController()
        {
            _scenesManager = GameClient.Get<IScenesManager>();
            _userProfileManager = GameClient.Get<IUserProfileManager>();
            _characterModels = new List<RotateModel>();
            _scenesManager.SceneForAppStateWasLoadedEvent += SceneForAppStateWasLoadedEvent;
            _userProfileManager.Inventory.InventoryUpdatedEvent += InventoryUpdatedEventEventHandler;
            
            _clickableLayer = LayerMask.NameToLayer("RotateModel");
            GetCamera();
        }

        public void Init()
        {
            var gameObject = GameObject.Find("ModelRender/Characters");
           
            _modelsParent = gameObject.transform;
            var loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            foreach (var path in _characterPrefabPaths)
            {
                var objectByPath = loadObjectsManager.GetObjectByPath<GameObject>(path);
                var sceneObject = MonoBehaviour.Instantiate(objectByPath, _modelsParent);
                _characterModels.Add(new RotateModel(sceneObject, _rotateSpeed));
            }
        }

        public void SetMainModelPosition()
        {
            if(_currentModel!=null)
                _currentModel.SetMainPosition();
        }

        public void SetCanHandleInput(bool state)
        {
            _canHandleInput = state;
        }

        public void SetCurrentModelMenu(int characterID)
        {
            SetModel(characterID);
        }

        public void SetCurrentModelInventory(int characterID)
        {
            if(!_userProfileManager.Inventory.IsInventoryInitialized)
                return;
            
            SetModel(characterID);
            _userProfileManager.Inventory.TriggerChangedCharacter(characterID);
        }

        public void Update()
        {
            if (!_canHandleInput)
                return;
            HandleInput();
        }

        public void ResetAll()
        {
            
        }

        public void Dispose()
        {
            _scenesManager.SceneForAppStateWasLoadedEvent -= SceneForAppStateWasLoadedEvent;
        }

        private void SetModel(int characterID)
        {
            if (_currentModel != null)
                _currentModel.SetActive(false);
            _characterModels[characterID].SetActive(true);
            _currentModel = _characterModels[characterID];
            _currentModel.SetMainPosition();
        }

        private void InventoryUpdatedEventEventHandler()
        {
            Init();
        }

        private void SceneForAppStateWasLoadedEvent(Enumerators.AppState state)
        {
            // Debug.Log($"SCENE LOADED EVENT");
            if (state == Enumerators.AppState.Main)
                GetCamera();
            if(state == Enumerators.AppState.Game)
                _canHandleInput = false;
        }

        private void GetCamera()
        {
            _camera = GameObject.Find("ModelCamera").GetComponent<Camera>();
            _canHandleInput = true;
        }

        private void HandleInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << _clickableLayer))
                    if (hit.transform.gameObject == _currentModel.GetSelfObject())
                    {
                        _isRotating = true;
                        _startMousePosition = Input.mousePosition;
                    }
            }

            if (_isRotating)
            {
                Vector3 delta = Input.mousePosition - _startMousePosition;
                _startMousePosition = Input.mousePosition;

                _currentModel.Rotate(delta);
            }

            if (Input.GetMouseButtonUp(0)) 
                _isRotating = false;
        }
    }
    
    public class RotateModel
    {
        private GameObject _selfObject;
        private float _rotateSpeed;
        private readonly Vector3 _defaultRotationValue = new Vector3(0, 180, 0);

        public RotateModel(GameObject selfObject, float rotateSpeed)
        {
            _selfObject = selfObject;
            _rotateSpeed = rotateSpeed;
        }

        public void Rotate(Vector3 deltaRotation)
        {
            _selfObject.transform.Rotate(Vector3.up, deltaRotation.x * -_rotateSpeed * Time.deltaTime);
        }

        public void SetMainPosition()
        {
            _selfObject.transform.localEulerAngles = _defaultRotationValue;
        }

        public void SetActive(bool state) => _selfObject.SetActive(state);

        public GameObject GetSelfObject() => _selfObject;
    }
}