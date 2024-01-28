using Cinemachine;
using DG.Tweening;
using GrandDevs.Networking;
using GrandDevs.Tavern.Helpers;
using UnityEngine;
using Enumerators = GrandDevs.Tavern.Common.Enumerators;

namespace GrandDevs.Tavern
{
    public class CameraController: IController
    {
        private IScenesManager _scenesManager;
        private GameplayController _gameplayController;
        private RoomController _roomController;
        private CinemachineBrain _cameraBrain;
        private CinemachineVirtualCamera _mainCamera;
        private CinemachineVirtualCamera _targetCamera;
        private Quaternion _defaultRotation;
        private readonly float _zoomDuration = .5f;
        private readonly float _delayToZoomOut = .8f;
        private int _defaultCellRange = 1;
        private int _sideCameraOffset = -3;
        private string _mainCameraFrontPath = "Cameras/DefaultCameraFront";
        private string _mainCameraSidePath = "Cameras/DefaultCameraSide";
        private string _targetCameraPath = "Cameras/TargetCamera";
        private int _currentLevel;

        public void Init()
        {
            var gameplayManager = GameClient.Get<IGameplayManager>();
            _gameplayController = gameplayManager.GetController<GameplayController>();
            _roomController = gameplayManager.GetController<RoomController>();
            _gameplayController.OnPlayersSpawned += PlayersSpawnedEventHandler;
            _roomController.OnJoinedRoom += JoinedRoomEventHandler;
            _scenesManager = GameClient.Get<IScenesManager>();
            _scenesManager.SceneForAppStateWasLoadedEvent += SceneForAppStateWasLoadedEvent;
        }

        private void SceneForAppStateWasLoadedEvent(Enumerators.AppState state)
        {
            if (state == Enumerators.AppState.Game)
            {
                _cameraBrain = GameObject.FindFirstObjectByType<CinemachineBrain>();
                if(_currentLevel == 0)
                    _mainCamera = GameObject.Find(_mainCameraFrontPath).transform.GetComponent<CinemachineVirtualCamera>(); 
                if(_currentLevel == 1)
                    _mainCamera = GameObject.Find(_mainCameraSidePath).transform.GetComponent<CinemachineVirtualCamera>();
                _cameraBrain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
                _mainCamera.Priority = 11;
                _targetCamera = GameObject.Find(_targetCameraPath).transform.GetComponent<CinemachineVirtualCamera>();
            }
        }

        private void JoinedRoomEventHandler(RoomInfo data) => _currentLevel = data.level;
        
        public void FocusCamera(Cell attackerCell, Cell targetCell, int attackRange)
        {
            _targetCamera.transform.rotation = _mainCamera.transform.rotation;
            _targetCamera.transform.position = _mainCamera.transform.position;
            var attackerPos = attackerCell.GetWorldPosition();
            var targetPos = targetCell.GetWorldPosition();
            
            bool isAttackerCloser = attackerCell.Y < targetCell.Y;
            
            Vector3 focusPosition = isAttackerCloser ? attackerPos : targetPos;
            var offsetX = _currentLevel == 0 ? 0 : _sideCameraOffset;
            var cameraOffset = Constants.CameraOffset  + new Vector3(offsetX, attackRange, -attackRange * .5f);
            var cameraTarget = focusPosition + cameraOffset;
            
            // Debug.Log($"ATTACKER CLOSER: {isAttackerCloser} " +
                      // $"attacker Y: {attackerCell.Y} target Y: {targetCell.Y}");
            
            _targetCamera.transform.DOMove(cameraTarget, _zoomDuration)
                .OnComplete(() => InternalTools.DoActionDelayed(SetMainCameraPriority, _delayToZoomOut));

            SetTargetCameraPriority();
        }



        public void Update()
        {
        }

        public void Dispose()
        {
            _scenesManager.SceneForAppStateWasLoadedEvent -= SceneForAppStateWasLoadedEvent;
            _roomController.OnJoinedRoom -= JoinedRoomEventHandler;
            _gameplayController.OnPlayersSpawned -= PlayersSpawnedEventHandler;
        }

        public void ResetAll()
        {
            _cameraBrain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
            _mainCamera.Priority = 11;
        }
        
        private void PlayersSpawnedEventHandler()
        {
            // SetMainCameraPriority();
        }

        private void MyLookAt(Transform cameraTransform, Vector3 targetPosition)
        {
            Vector3 relativePos = targetPosition - cameraTransform.position;
            Quaternion rotation = Quaternion.LookRotation(relativePos);
            cameraTransform.rotation = rotation;
        }

        private void SetMainCameraPriority()
        {
            _cameraBrain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
            _mainCamera.Priority = 11;
            _targetCamera.Priority = 10;
            _targetCamera.LookAt = null;
        }

        private void SetTargetCameraPriority()
        {
            _cameraBrain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
            _mainCamera.Priority = 10;
            _targetCamera.Priority = 11;
        }

        public CinemachineVirtualCamera GetCameraMain() => _mainCamera;
    }
}