using RedBjorn.SuperTiles.Multiplayer.UI.Menu;
using RedBjorn.SuperTiles.Multiplayer.UI.Menu.States;
using UnityEngine;
using UnityEngine.UI;

namespace RedBjorn.SuperTiles.Multiplayer.UI
{
    /// <summary>
    /// UI panel which provides UI main menu elements
    /// </summary>
    public class MenuUI : MonoBehaviour
    {
        public GameObject Buttons;
        public Button SingleplayerButton;
        public GameObject Singleplayer;
        public Button MultiplayerButton;
        public MultiplayerUI Multiplayer;

        MenuUIState CurrentState;

        void Awake()
        {
            Singleplayer.SetActive(false);
            Multiplayer.Hide();
            Buttons.SetActive(false);
            SingleplayerButton.onClick.RemoveAllListeners();
            SingleplayerButton.onClick.AddListener(() => ChangeState(new SingleplayerRegime()));
            MultiplayerButton.onClick.RemoveAllListeners();
            MultiplayerButton.onClick.AddListener(() => ChangeState(new MultiplayerRegime()));

            if (NetworkController.IsConnected)
            {
                ChangeState(new MultiplayerRegime());
            }
            else
            {
                ChangeState(new Idle());
            }
        }

        public void ChangeState(MenuUIState state)
        {
            if (CurrentState != null)
            {
                CurrentState.OnExit();
            }
            CurrentState = state;
            if (CurrentState != null)
            {
                CurrentState.Enter(this);
            }
        }
    }
}
