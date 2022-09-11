using RedBjorn.SuperTiles.UI;

namespace RedBjorn.SuperTiles.Multiplayer.UI.Multiplayer
{
    /// <summary>
    /// Base class for multiplayer UI state machine
    /// </summary>
    public abstract class MultiplayerUIState
    {
        protected MultiplayerUI Controller;
        public void Enter(MultiplayerUI controller)
        {
            Controller = controller;
            OnEnter();
        }

        public virtual void OnEnter() { }
        public virtual void OnExit() { }

        public virtual void OnRoomJoined() { }
        public virtual void OnRoomJoinFailed() { }
        public virtual void OnRoomLeft() { }
        public virtual void OnRoomCreated() { }
        public virtual void OnRoomCreateFailed() { }
        public virtual void OnConnected() { }

        public virtual void OnDisconnected()
        {
            if (!Controller.IsQuitting)
            {
                var text = $"Network issue";
                ConfirmMessageUI.Show(text,
                                    "OK",
                                    null,
                                    () =>
                                    {
                                        ChangeState(new States.Login());
                                    },
                                    null);
            }
        }

        public virtual void OnRoomListUpdate() { }

        public void ChangeState(MultiplayerUIState state)
        {
            Controller.ChangeState(state);
        }
    }
}
