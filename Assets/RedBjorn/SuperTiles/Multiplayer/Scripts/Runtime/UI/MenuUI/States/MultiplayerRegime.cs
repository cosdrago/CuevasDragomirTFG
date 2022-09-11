namespace RedBjorn.SuperTiles.Multiplayer.UI.Menu.States
{
    /// <summary>
    /// Menu UI state which dispays UI when multiplayer regime is selected
    /// </summary>
    public class MultiplayerRegime : MenuUIState
    {
        public override void OnEnter()
        {
            Controller.Buttons.SetActive(true);
            Controller.Multiplayer.Show();
        }

        public override void OnExit()
        {
            Controller.Buttons.SetActive(false);
            Controller.Multiplayer.Hide();
        }
    }
}
