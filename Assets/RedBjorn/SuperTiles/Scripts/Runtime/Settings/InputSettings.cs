using RedBjorn.Utils;
using UnityEngine;

namespace RedBjorn.SuperTiles.Settings
{
    /// <summary>
    /// Settings which contains data about keycodes which is paired to corresponding input actions
    /// </summary>
    public class InputSettings : ScriptableObjectExtended
    {
        public KeyCode CancelItem = KeyCode.Mouse1;
        public KeyCode CompleteTurn = KeyCode.Space;
        public KeyCode Menu = KeyCode.Escape;
        public KeyCode DebugUI = KeyCode.F6;
        public KeyCode SubmitMain = KeyCode.Space;
        public KeyCode SubmitAlt = KeyCode.Return;
        public KeyCode Cancel = KeyCode.Escape;
        public KeyCode CameraClockwise = KeyCode.E;
        public KeyCode CameraCounterClockwise = KeyCode.Q;
    }
}
