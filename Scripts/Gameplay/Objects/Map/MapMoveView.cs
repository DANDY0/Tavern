using System;
using DG.Tweening;
using GrandDevs.Tavern;
using UnityEngine;

namespace GrandDevs.Networking
{
    public class MapMoveView
    {
        private GameObject _selfObject;
        private GameObject _direction;
        private RectTransform _playerIndicator;
        private RectTransform _directionRectTransform;

        private MapMoveController _controller;

        private readonly int _mapMovementDuration;
        
        public MapMoveView(GameObject selfObject, MapMoveController controller, int mapMovementDuration)
        {
            _selfObject = selfObject;
            _controller = controller;
            _mapMovementDuration = mapMovementDuration;

            _direction = _selfObject.transform.Find("Direction").gameObject;
            _playerIndicator = _direction.transform.Find("PlayerIndicator") as RectTransform;
            _directionRectTransform = _direction.GetComponent<RectTransform>();
        }

        public void SetIndicatorPosition(Vector3 indicatorPos)
        {
            _playerIndicator.position = indicatorPos;
        }

        public void ShowMoveDirection(MapLocation targetLocation, MapLocation currentLocation)
        {
            Vector2 directionVector = targetLocation.GetRectTransform().anchoredPosition - currentLocation.GetRectTransform().anchoredPosition;
            float angle = Mathf.Atan2(directionVector.y, directionVector.x) * Mathf.Rad2Deg;
            float distance = directionVector.magnitude;

            _direction.transform.rotation = Quaternion.Euler(0, 0, angle);
            _directionRectTransform.sizeDelta = new Vector2(distance, _directionRectTransform.sizeDelta.y);
            _direction.transform.position = (currentLocation.GetTransform().position + targetLocation.GetTransform().position) / 2;
            // _direction.SetActive(true);
        }

        public void HideMoveDirection()
        {
            _directionRectTransform.sizeDelta = new Vector2(0, _directionRectTransform.sizeDelta.y);
            // _direction.SetActive(false);
        }
        
        public void PlayerMoveAnimation(Action callback, float elapsedTime = 0)
        {
            _playerIndicator.anchoredPosition = new Vector3(_playerIndicator.anchoredPosition.x, 0, 0);
            _playerIndicator.localEulerAngles = new Vector3(0, 0, - _directionRectTransform.localEulerAngles.z);

            float totalDistance = _directionRectTransform.sizeDelta.x;
            float currentDistance = Mathf.Lerp(-totalDistance / 2, totalDistance / 2, elapsedTime / _mapMovementDuration);
    
            _playerIndicator.anchoredPosition = new Vector2(currentDistance, _playerIndicator.anchoredPosition.y);

            float remainingTime = _mapMovementDuration - elapsedTime;
            _playerIndicator.DOLocalMoveX(totalDistance / 2, remainingTime).SetEase(Ease.Linear) .OnComplete(() => callback?.Invoke());;
        }

    }
}