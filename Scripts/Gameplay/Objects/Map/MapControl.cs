using UnityEngine;

namespace GrandDevs.Tavern
{
    public class MapControl
    {
        private const float MinScale = 1.0f;
        private const float MaxScale = 2.0f;
        private const float ZoomSpeed = 0.15f;

        private RectTransform _mapRect;
        private Vector3 _originalScale;
        private Vector2 _lastMousePos;

        public MapControl(RectTransform mapRect)
        {
            _mapRect = mapRect;
            _originalScale = mapRect.localScale;
        }

        public void Update()
        {
            HandleDrag();
            HandleZoom();
        }

        private void HandleDrag()
        {
            if (Input.GetMouseButton(0))
            {
                Vector2 currentMousePos = Input.mousePosition;
                if (_lastMousePos == Vector2.zero)
                {
                    _lastMousePos = currentMousePos;
                }

                Vector2 delta = currentMousePos - _lastMousePos;
                _mapRect.anchoredPosition += delta;
                ClampPosition();

                _lastMousePos = currentMousePos;
            }
            else
            {
                _lastMousePos = Vector2.zero;
            }
        }

        private void HandleZoom()
        {
            float scrollDelta = Input.mouseScrollDelta.y;
            Vector3 newScale = _mapRect.localScale +
                               new Vector3(scrollDelta * ZoomSpeed, scrollDelta * ZoomSpeed, 1.0f);

            newScale.x = Mathf.Clamp(newScale.x, _originalScale.x * MinScale, _originalScale.x * MaxScale);
            newScale.y = Mathf.Clamp(newScale.y, _originalScale.y * MinScale, _originalScale.y * MaxScale);

            _mapRect.localScale = newScale;
            ClampPosition();
        }

        private void ClampPosition()
        {
            Vector2 position = _mapRect.anchoredPosition;

            float widthDiff = (_mapRect.localScale.x - _originalScale.x) * _mapRect.sizeDelta.x;
            float heightDiff = (_mapRect.localScale.y - _originalScale.y) * _mapRect.sizeDelta.y;

            position.x = Mathf.Clamp(position.x, -widthDiff / 2, widthDiff / 2);
            position.y = Mathf.Clamp(position.y, -heightDiff / 2, heightDiff / 2);

            _mapRect.anchoredPosition = position;
        }
    }
}