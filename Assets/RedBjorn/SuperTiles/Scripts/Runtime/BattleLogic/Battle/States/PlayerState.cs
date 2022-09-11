using System.Linq;

namespace RedBjorn.SuperTiles.Battle.States
{
    /// <summary>
    /// BattleView state which is a initial state for Player loop
    /// </summary>
    public class PlayerState : State
    {
        protected override void Enter()
        {
            //Update UI
            Controller.TeamPanelUI.Init(Player.Squad, Unit, (unit) => Controller.TryUnitSelect(unit));
            Controller.ItemsUI.UpdateItems(Controller, (item) => ChangeState(new UnitItemState(item)));
        }

        public override void Update()
        {
            //Try complete turn
            if (Controller.TryTurnFinish())
            {
                return;
            }

            //Try select unit
            if (InputController.GetOnWorldUp)
            {
                Controller.TryUnitSelect(InputController.GroundPosition);
            }

            //Check valid unit conditions
            if (Unit == null || Unit.IsDead)
            {
                Unit = Player.Squad.FirstOrDefault(u => !u.IsDead);
                if (Unit == null)
                {
                    Log.E($"Coundn't find any player unit alive. Go to {nameof(IdleState)}");
                    ChangeState(new IdleState());
                    return;
                }
            }

            //Check if can do MoveAction
            if (Battle.Level.Actions.CanMove(Unit, Battle))
            {
                ChangeState(new UnitMoveState());
                return;
            }
        }

        public override void Exit()
        {

        }

        public override bool IsSaveable()
        {
            return Game.Loader != null;
        }
    }
}