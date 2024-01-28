using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using GrandDevs.Tavern.Common;
using UnityEngine;

namespace GrandDevs.Tavern
{
    public class PlayerCamerasController: IController
    {
        private IAppStateManager _appStateManager;
        private GameplayController _gameplayController;
        private CinemachineBrain _cameraBrain;
        private Camera _camera;
        private List<Player> _players = new List<Player>();
        private CinemachineVirtualCamera _mainCamera;
        private readonly string _alphaString = "Alpha";
        private readonly string _defaultCameraPath = "Cameras/DefaultCamera";
        private int _layerMask;

        private const int CameraPriorityValue = 12;
        private const int MainPriorityValue = 11;
        private const int LessPriorityValue = 10;

        public void Init()
        {
            _appStateManager = GameClient.Get<IAppStateManager>();
            _gameplayController = GameClient.Get<IGameplayManager>().GetController<GameplayController>();
            _gameplayController.OnPlayersSpawned += PlayersSpawnedEventHandler;
            _layerMask = 1 << LayerMask.NameToLayer("SpriteUI");
        }

        public void Update()
        {
            if (_appStateManager.AppState != Enumerators.AppState.Game)
                return;

            bool isShiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            for (int i = 0; i < 4; i++)
            {
                if (Input.GetKeyDown((KeyCode)Enum.Parse(typeof(KeyCode), _alphaString + (i + 1))))
                {
                    if (isShiftPressed)
                        SetThirdPersonCameraPriority(i);
                    else
                        SetTargetCameraPriority(i);
                }
        
                if (Input.GetKeyUp((KeyCode)Enum.Parse(typeof(KeyCode), _alphaString + (i + 1))))
                {
                    SetMainCameraPriority();
                }
            }
        }

        public void Dispose() { }

        public void ResetAll() => SetMainCameraPriority();

        private void PlayersSpawnedEventHandler()
        {
            _cameraBrain = GameObject.FindFirstObjectByType<CinemachineBrain>();
            _camera = _cameraBrain.gameObject.GetComponent<Camera>();
            var cameraController = GameClient.Get<IGameplayManager>().GetController<CameraController>();
            // _mainCamera = GameObject.Find(_defaultCameraPath).GetComponent<CinemachineVirtualCamera>(); 
            _mainCamera = cameraController.GetCameraMain();
            foreach (var player in _gameplayController.GetPlayers())
                _players.Add(player);
        }

        private void SetMainCameraPriority()
        {
            _cameraBrain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
            _mainCamera.Priority = MainPriorityValue;
            ToggleLayers(true);

            foreach (var player in _players)
            {
                player.View.GetCameraFirst().Priority = LessPriorityValue;
                player.View.GetCameraThird().Priority = LessPriorityValue;
            }
        }

        private void SetTargetCameraPriority(int playerID)
        {
            _cameraBrain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
            _mainCamera.Priority = LessPriorityValue;
            ToggleLayers(false);

            foreach (var player in _players)
                player.View.GetCameraFirst().Priority = (player.GetPlayerData().PlayerID == playerID) ? CameraPriorityValue : LessPriorityValue;
        }

        private void SetThirdPersonCameraPriority(int playerID)
        {
            _cameraBrain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
            _mainCamera.Priority = LessPriorityValue;
            ToggleLayers(false);
            foreach (var player in _players)
            {
                if (player.GetPlayerData().PlayerID == playerID)
                    player.View.GetCameraThird().Priority = CameraPriorityValue;
                else
                    player.View.GetCameraFirst().Priority = LessPriorityValue;
            }
        }

        private void ToggleLayers(bool enable)
        {
            if (enable)
                _camera.cullingMask |= _layerMask;
            else
                _camera.cullingMask &= ~_layerMask;
        }

    }
}
