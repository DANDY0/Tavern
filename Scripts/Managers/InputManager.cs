using System;
using System.Collections.Generic;
using GrandDevs.Tavern.Common;
using UnityEngine;

namespace GrandDevs.Tavern
{
    public class InputManager : IService, IInputManager
    {
        private readonly float SwipeDeltaY;
        private readonly float SwipeDeltaX;

        private readonly object _sync = new object();

        private List<InputEvent> _inputHandlers;

        private int _customFreeIndex;

        private Vector3 _startTouchPosition;

        public bool CanHandleInput { get; set; }

        public List<Rect> BlindArea { get; set; }

        public InputManager()
        {
            SwipeDeltaY = Screen.height / 6f;
            SwipeDeltaX = Screen.width / 6f;
        }

        /// <summary>
        ///     Registers the input handler by type and code.
        /// </summary>
        /// <returns>The input handler.</returns>
        /// <param name="type">Type.</param>
        /// <param name="inputCode">Input code.</param>
        /// <param name="onInputUp">On input up.</param>
        /// <param name="onInputDown">On input down.</param>
        /// <param name="onInput">On input.</param>
        public int RegisterInputHandler(
            Enumerators.InputType type, int inputCode, Action onInputUp = null, Action onInputDown = null,
            Action onInput = null, Action<object> onInputEndParametrized = null)
        {
            lock (_sync)
            {
                InputEvent item = new InputEvent
                {
                    Code = inputCode,
                    InputCallback = onInput,
                    InputDownCallback = onInputDown,
                    InputUpCallback = onInputUp,
                    InputParametrizedCallback = onInputEndParametrized,
                    Type = type,
                    Index = _customFreeIndex++
                };

                _inputHandlers.Add(item);

                return item.Index;
            }
        }

        public void UnregisterInputHandler(int index)
        {
            lock (_sync)
            {
                InputEvent inputHandler = _inputHandlers.Find(x => x.Index == index);

                if (inputHandler != null)
                {
                    _inputHandlers.Remove(inputHandler);
                }
            }
        }

        public bool TouchInBlindArea(Vector2 position)
        {
            foreach (Rect rect in BlindArea)
            {
                if (rect.Contains(position))
                    return true;
            }

            return false;
        }

        public void Init()
        {
            CanHandleInput = true;

            _inputHandlers = new List<InputEvent>();

            BlindArea = new List<Rect>();
        }

        public void Update()
        {
            if (CanHandleInput)
            {
                if (_inputHandlers.Count > 0)
                {
                    lock (_sync)
                    {
                        HandleInput();
                    }
                }
            }
        }

        public void Dispose()
        {
            _inputHandlers.Clear();
        }

        private void HandleInput()
        {
            InputEvent item;
            for (int i = 0; i < _inputHandlers.Count; i++)
            {
                item = _inputHandlers[i];

                switch (item.Type)
                {
                    case Enumerators.InputType.Mouse:
                        {
                            if (Input.GetMouseButton(item.Code))
                            {
                                item.InvokeInputCallback();
                            }

                            if (Input.GetMouseButtonUp(item.Code))
                            {
                                item.InvokeInputUpCallback();
                            }

                            if (Input.GetMouseButtonDown(item.Code))
                            {
                                item.InvokeInputDownCallback();
                            }
                        }
                        break;
                    case Enumerators.InputType.Keyboard:
                        {
                            if (Input.GetKey((KeyCode)item.Code))
                            {
                                item.InvokeInputCallback();
                            }

                            if (Input.GetKeyUp((KeyCode)item.Code))
                            {
                                item.InvokeInputUpCallback();
                            }

                            if (Input.GetKeyDown((KeyCode)item.Code))
                            {
                                item.InvokeInputDownCallback();
                            }
                        }
                        break;
                    case Enumerators.InputType.Swipe:
                        {
                            if (Input.GetMouseButtonDown(item.Code) && !item.Started)
                            {
                                _startTouchPosition = Input.mousePosition;

                                item.Started = true;
                            }

                            if (Input.GetMouseButtonUp(item.Code) && item.Started)
                            {
                                Vector3 delta = _startTouchPosition - Input.mousePosition;
                                Vector3 normalized = delta.normalized;

                                if (Mathf.Abs(normalized.y) > Mathf.Abs(normalized.x) && Mathf.Abs(delta.y) > SwipeDeltaY)
                                {
                                    item.InvokeInputParametrizedCallback(normalized.y < 0 ? Enumerators.Direction.Up : Enumerators.Direction.Down);
                                }
                                else if(Mathf.Abs(delta.x) > SwipeDeltaX)
                                {
                                    item.InvokeInputParametrizedCallback(normalized.x < 0 ? Enumerators.Direction.Right : Enumerators.Direction.Left);
                                }

                                item.Started = false;
                            }
                        }
                        break;
                }
            }
        }
    }

    public class InputEvent
    {
        public int Index { get; set; }

        public int Code { get; set; }

        public bool Started { get; set; }

        public Enumerators.InputType Type;

        public Action InputUpCallback;

        public Action InputDownCallback;

        public Action InputCallback;

        public Action<object> InputParametrizedCallback;

        public void InvokeInputUpCallback()
        {
            InputUpCallback?.Invoke();
        }

        public void InvokeInputDownCallback()
        {
            InputDownCallback?.Invoke();
        }

        public void InvokeInputCallback()
        {
            InputCallback?.Invoke();
        }

        public void InvokeInputParametrizedCallback(object param)
        {
            InputParametrizedCallback?.Invoke(param);
        }
    }
}
