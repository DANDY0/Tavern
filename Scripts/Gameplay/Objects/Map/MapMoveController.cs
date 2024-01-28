using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using External.Essentials.GrandDevs.SocketIONetworking.Scripts;
using GrandDevs.Networking;
using GrandDevs.Tavern.Helpers;
using UnityEngine;
using Enumerators = GrandDevs.Tavern.Common.Enumerators;

namespace GrandDevs.Tavern
{
    public class MapMoveController
    {
        private readonly ApiRequestHandler _apiRequestHandler;
        private MapMoveView _mapMoveView;
        
        private Dictionary<int, MapLocation> _mapLocations = new Dictionary<int, MapLocation>();
        private MapLocation _currentSelectLocation;
        private MapLocation _currentLocation;
        private MapLocation _targetLocation;
        private MapActionsHandler _mapActionsHandler;
        
        private readonly int _mapMovementDuration;
        private bool _isMoving;

        public MapMoveController(GameObject selfObject, Dictionary<int, MapLocation> mapLocations, int mapMovementDuration)
        {
            _mapLocations = mapLocations;
            _mapMovementDuration = mapMovementDuration;
            _apiRequestHandler = GameClient.Get<INetworkManager>().APIRequestHandler;
            _mapMoveView = new MapMoveView(selfObject, this, _mapMovementDuration);
            _mapActionsHandler = new MapActionsHandler();
            
            InitLocations();
        }

        private async void InitLocations()
        {
            foreach (var mapLocation in _mapLocations)
            {
                if (mapLocation.Value.IsCurrent())
                {
                    _currentLocation = mapLocation.Value;
                    _mapMoveView.SetIndicatorPosition(_currentLocation.GetRectTransform().position);
                }

                mapLocation.Value.Init(this);
                mapLocation.Value.SelectEvent += SelectHandler;
                mapLocation.Value.GoEvent += GoHandler;
            }

            var meOnMapData = await _apiRequestHandler.GetMeOnMapAsync();
            SetCurrentMapData(meOnMapData);
            await CheckAndSkipBattle(meOnMapData);
        }

        private async Task CheckAndSkipBattle(APIModel.GetMeOnMapResponse getMeOnMapResponse)
        {
            var battleAction = getMeOnMapResponse.result.details.FirstOrDefault(detail =>
            {
                var detailType = InternalTools.EnumFromString<Enumerators.MapActions>(detail.type);
                return detailType == Enumerators.MapActions.Battle && !detail.status;
            });
            
            if (battleAction != null)
            {
                
                var passBattleResponse = await _apiRequestHandler.PassBattleAsync(int.Parse(battleAction.id), true);
            }
        }

        private void SetCurrentMapData(APIModel.GetMeOnMapResponse getMeOnMapResponse)
        {
            _mapLocations[getMeOnMapResponse.result.position].SetCurrent(true);
            
            if (getMeOnMapResponse.result.position == getMeOnMapResponse.result.targetPosition)
            {
                SetPlayerStaticPosition(getMeOnMapResponse.result.position);
                if (getMeOnMapResponse.result.details != null && getMeOnMapResponse.result.details.Count > 0)
                {
                    var resultDetail = getMeOnMapResponse.result.details[0];
                    if(!resultDetail.status)
                        _mapActionsHandler.HandleAction(resultDetail);
                }
            }
            else
            {
                if(getMeOnMapResponse.result.details!=null && getMeOnMapResponse.result.details.Count>0)
                    _mapActionsHandler.SetCurrentMapAction(getMeOnMapResponse.result.details[0]); 
                
                double currentUnixTime = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
                float elapsedTime = (float)(_mapMovementDuration - (getMeOnMapResponse.result.reachAt - currentUnixTime));

                if (elapsedTime < 0)
                    elapsedTime = 0;

                ContinuePlayerMovement(getMeOnMapResponse.result.position, getMeOnMapResponse.result.targetPosition, elapsedTime);
            }
        }

        private void SetPlayerStaticPosition(int position)
        {
            _isMoving = false;
            _mapMoveView.HideMoveDirection();

            if (_mapLocations.TryGetValue(position, out var currentLocation))
            {
                _currentLocation = currentLocation;

                // float halfWidth = _directionRectTransform.sizeDelta.x / 2;
                // _playerIndicator.anchoredPosition = new Vector2(-halfWidth, _playerIndicator.anchoredPosition.y);
                _mapMoveView.SetIndicatorPosition(_currentLocation.GetRectTransform().position);

                _currentLocation.SetCurrent(true);
            }
        }

        private void ContinuePlayerMovement(int currentPosition, int targetPosition, float elapsedTime)
        {
            _isMoving = true;

            if (_mapLocations.TryGetValue(currentPosition, out var currentLocation))
            {
                _currentLocation = currentLocation;
            }

            if (_mapLocations.TryGetValue(targetPosition, out var targetLocation))
            {
                _targetLocation = targetLocation;
                _mapMoveView.ShowMoveDirection(_targetLocation, _currentLocation);
                _mapMoveView.PlayerMoveAnimation(OnFinishTravel, elapsedTime);
            }
        }

        private void SelectHandler(MapLocation selectLocation)
        {
            if(_isMoving || (_currentSelectLocation!=null && _currentSelectLocation == selectLocation))
                return;
            if(_currentSelectLocation !=null)
                _currentSelectLocation.EnableUI(false);
            _currentSelectLocation = selectLocation;
            _currentSelectLocation.EnableUI(true);
        }

        private async void GoHandler(MapLocation targetLocation)
        {
             APIModel.TravelOnMapResponse travelOnMapResponse 
                 = await _apiRequestHandler.TravelOnMapAsync(targetLocation.LocationIndex);

             if(!travelOnMapResponse.status)
                 return;

             if (travelOnMapResponse.result.actions != null && travelOnMapResponse.result.actions.Count>0)
             {
                 if (travelOnMapResponse.result.actions != null && travelOnMapResponse.result.actions.Count > 0)
                 {
                     var action = travelOnMapResponse.result.actions[0];
                     _mapActionsHandler.SetCurrentMapAction(action); 
                     Debug.LogError($"Action: {action.id} Type: {action.type}");
                 }
             }

             Move(_mapLocations[targetLocation.LocationIndex ]);
        }

        private void Move(MapLocation targetLocation)
        {
            _isMoving = true;
            SetTargetLocation(targetLocation);
            _mapMoveView.ShowMoveDirection(_targetLocation, _currentLocation);
            _mapMoveView.PlayerMoveAnimation(OnFinishTravel);
        }

        private void SetTargetLocation(MapLocation targetLocation)
        {
            _targetLocation = targetLocation;
        }

        
        private void OnFinishTravel()
        {
            _currentLocation.SetCurrent(false);
            _currentLocation = _targetLocation;
            _currentLocation.SetCurrent(true);
            
            _mapMoveView.HideMoveDirection();
            _currentLocation.EnableUI(false);
            
            _isMoving = false;
            
            _mapActionsHandler.HandleCurrentAction();
        }
    }

}