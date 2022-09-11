namespace RedBjorn.SuperTiles.Battle.States
{
    /// <summary>
    /// BattleView state when Player observe playing scheduled IAction
    /// </summary>
    public class SpectatorState : State
    {
        protected override void Enter()
        {
            UpdateUI();
        }

        public override void Update()
        {
            if (InputController.GetOnWorldUp)
            {
                Controller.TryUnitSelect(InputController.GroundPosition);
            }
        }

        public override void Exit() { }

        public override void OnTurnStarted()
        {
            if (Controller.IsMyPlayer)
            {
                Controller.Status = Controller.Statuses.TurnMy;
                UI.TurnPlayerMyUI.Show(() =>
                {
                    ChangeState(new PlayerState());
                    Battle.Player.OnMyTurnstarted();
                });
            }
            else
            {
                Controller.Status = string.Format(Controller.Statuses.TurnOther, Controller.Battle.Player.Nickname);
                UI.TurnPlayerOtherUI.Show(() =>
                {
                    Battle.Player.OnMyTurnstarted();
                });
            }
        }

        public override void OnUnitChanged()
        {
            UpdateUI();
        }

        public override void OnTurnFinishStarted()
        {

        }

        void UpdateUI()
        {
            if (Player != null)
            {
                Controller.TeamPanelUI.Init(Player.Squad, Unit, (unit) => Controller.TryUnitSelect(unit));
                Controller.ItemsUI.UpdateItems(Controller, null);
            }
        }
    }
}
