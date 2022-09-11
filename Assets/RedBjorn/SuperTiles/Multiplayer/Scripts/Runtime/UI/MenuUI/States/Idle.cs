namespace RedBjorn.SuperTiles.Multiplayer.UI.Menu.States
{
    /// <summary>
    /// Initial menu UI state
    /// </summary>
    public class Idle : MenuUIState
    {
        public override void OnEnter()
        {
            Controller.Singleplayer.SetActive(false);
            Controller.Multiplayer.Hide();
            Controller.Buttons.SetActive(true);
        }

        public override void OnExit()
        {
            Controller.Buttons.SetActive(false);
        }
    }
}
