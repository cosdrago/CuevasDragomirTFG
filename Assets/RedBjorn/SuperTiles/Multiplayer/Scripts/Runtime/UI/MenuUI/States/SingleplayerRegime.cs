namespace RedBjorn.SuperTiles.Multiplayer.UI.Menu.States
{
    /// <summary>
    /// Menu UI state which dispays UI when singleplayer regime is selected
    /// </summary>
    public class SingleplayerRegime : MenuUIState
    {
        public override void OnEnter()
        {
            Controller.Buttons.SetActive(true);
            Controller.Singleplayer.SetActive(true);
        }

        public override void OnExit()
        {
            Controller.Buttons.SetActive(false);
            Controller.Singleplayer.SetActive(false);
        }
    }
}
