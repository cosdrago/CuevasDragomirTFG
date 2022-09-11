namespace RedBjorn.SuperTiles.Multiplayer.UI.Multiplayer.States
{
    /// <summary>
    /// The initial multiplayer UI state which displays UI when player need to log in
    /// </summary>
    public class Login : MultiplayerUIState
    {
        public override void OnEnter()
        {
            Controller.Menu.Multiplayer.gameObject.SetActive(true);
            Controller.Menu.Buttons.gameObject.SetActive(true);
            Controller.Connected.Hide();
            Controller.InProccess.SetActive(false);

            if (PlayerProfile.IsValid() && NetworkController.IsConnected)
            {
                OnLoginExist();
            }
            else
            {
                var login = Controller.Login;
                login.LoginButton.onClick.RemoveAllListeners();
                login.LoginButton.onClick.AddListener(OnLoginClicked);
                var servers = NetworkController.Servers;
                login.Show(PlayerProfile.PreNickname(), servers, NetworkController.ServerDefault);
            }
        }

        public override void OnExit()
        {
            Controller.Menu.Buttons.gameObject.SetActive(false);
            Controller.Login.Hide();
        }

        public override void OnDisconnected()
        {
            //Haven't connected yet
        }

        void OnLoginClicked()
        {
            PlayerProfile.SetNickname(Controller.Login.GetNickname());
            NetworkController.ServerDefault = Controller.Login.GetServername();
            OnLoginExist();
        }

        void OnLoginExist()
        {
            ChangeState(new Connecting());
        }
    }
}
