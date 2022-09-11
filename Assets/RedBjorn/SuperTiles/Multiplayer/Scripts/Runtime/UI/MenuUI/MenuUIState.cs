namespace RedBjorn.SuperTiles.Multiplayer.UI.Menu
{
    /// <summary>
    /// Base class for main menu UI state machine
    /// </summary>
    public abstract class MenuUIState
    {
        protected MenuUI Controller;
        public void Enter(MenuUI controller)
        {
            Controller = controller;
            OnEnter();
        }

        public virtual void OnEnter() { }
        public virtual void OnExit() { }

        public void ChangeState(MenuUIState state)
        {
            Controller.ChangeState(state);
        }
    }
}

