using System;
using GrandDevs.Tavern.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrandDevs.Tavern
{
    public class MapLocation
    {
        public event Action<MapLocation> SelectEvent;
        public event Action<MapLocation> GoEvent;
        public Enumerators.MapLocationType LocationType { get; }
        public int LocationIndex { get; }
        
        private GameObject _selfObject;
        private MapMoveController _mapMoveController;

        private TextMeshProUGUI _locationName;
        private Image _pointImage;
        private Button _selectButton;
        private Button _goButton;

        private bool _isCurrentLocation;
        
        public MapLocation(GameObject selfObject, Enumerators.MapLocationType locationType, int locationIndex)
        {
            _selfObject = selfObject;
            LocationType = locationType;
            LocationIndex = locationIndex;
            _locationName = _selfObject.transform.Find("Text_Location").GetComponent<TextMeshProUGUI>();
            _pointImage = _selfObject.transform.Find("Image_Point").GetComponent<Image>();
            _selectButton = _selfObject.transform.Find("Button_Select").GetComponent<Button>();
            _goButton = _selfObject.transform.Find("Button_Go").GetComponent<Button>();

            _selectButton.onClick.AddListener(SelectHandler);
            _goButton.onClick.AddListener(GoHandler);
        }

        public void Init(MapMoveController mapMoveController)
        {
            _mapMoveController = mapMoveController;
            _locationName.text = LocationType.ToString();
        }

        public Transform GetTransform() => _selfObject.transform;

        public void SetCurrent(bool state) => _isCurrentLocation = state;

        private void EnablePointer(bool state) => _pointImage.enabled = state;

        private void GoHandler()
        {
            EnableGoButton(false);
            GoEvent?.Invoke(this);
        }

        private void SelectHandler()
        {
            if(_isCurrentLocation)
                return;
            SelectEvent?.Invoke(this);
        }

        public void EnableUI(bool state)
        {
            EnableGoButton(state);
            EnablePointer(state);
        }
        
        private void EnableGoButton(bool state) => _goButton.gameObject.SetActive(state);
        
        public bool IsCurrent() => _isCurrentLocation;

        public RectTransform GetRectTransform() => _selfObject.transform as RectTransform;
    }
}