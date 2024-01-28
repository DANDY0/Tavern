using System.Collections.Generic;
using External.Essentials.GrandDevs.SocketIONetworking.Scripts;
using GrandDevs.Tavern.Common;
using GrandDevs.Tavern.Helpers;
using UnityEngine;

namespace GrandDevs.Tavern
{
    public class Map
    {
        private readonly ApiRequestHandler _apiRequestHandler;
        private GameObject _selfObject;

        private MapControl _mapControl;
        private Dictionary<int, MapLocation> _mapLocations = new Dictionary<int, MapLocation>();
        private MapMoveController _mapMoveController;

        public Map(GameObject selfObject)
        {
            _selfObject = selfObject;
            _apiRequestHandler = GameClient.Get<INetworkManager>().APIRequestHandler;
            _apiRequestHandler.GetApiConfigEvent += GetApiEventHandler;
        }

        private void GetApiEventHandler(APIModel.GameConfigData data)
        {
            int defaultLocationID = 0;
            var mapPieces = _selfObject.transform.Find("MapPieces");
            var locationsParent = mapPieces.transform.Find("Locations");
            var mapMove = mapPieces.transform.Find("MapMove").gameObject;

            foreach (var location in data.map)
            {
                var mapLocationType = InternalTools.EnumFromString<Enumerators.MapLocationType>(location.type);

                var locationGameObject = locationsParent.Find($"{location.type}_{location.id}").gameObject;

                if (locationGameObject != null)
                {
                    var mapLocation = new MapLocation(locationGameObject, mapLocationType, location.id);
                    /*if (location.id == defaultLocationID)
                        mapLocation.SetCurrent(true);*/
                    _mapLocations[location.id] = mapLocation;
                }
                else
                {
                    Debug.LogWarning($"Object with this name didnt found {location.type}_{location.id}!");
                }
            }

            _mapControl = new MapControl(mapPieces.GetComponent<RectTransform>());
            _mapMoveController = new MapMoveController(mapMove, _mapLocations, data.map_movement_duration);
        }


        public void Update()
        {
            if(_mapControl!= null)
                _mapControl.Update();
        }
    }
}