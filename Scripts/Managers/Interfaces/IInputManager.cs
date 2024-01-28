using GrandDevs.Tavern.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GrandDevs.Tavern
{
    public interface IInputManager
    {
        bool CanHandleInput { get; set; }

        List<Rect> BlindArea { get; set; }

        int RegisterInputHandler(
            Enumerators.InputType type, int inputCode, Action onInputUp = null, Action onInputDown = null,
            Action onInput = null, Action<object> onInputEndParametrized = null);

        void UnregisterInputHandler(int index);

        bool TouchInBlindArea(Vector2 position);
    }
}
