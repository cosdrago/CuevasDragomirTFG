using RedBjorn.SuperTiles.Battle.Actions;
using RedBjorn.SuperTiles.Items;

namespace RedBjorn.SuperTiles.Battle.States
{
    /// <summary>
    /// BattleView state which controls player ItemAction handling
    /// </summary>
    public class UnitItemState : State
    {
        ItemEntity Item;
        ITargetSelectorView ItemAbortable;

        UnitItemState() { }

        public UnitItemState(ItemEntity item)
        {
            Item = item;
        }

        protected override void Enter()
        {
            ItemSelectorStart();
            Controller.ItemsUI.Select(Item);
        }

        public override void Update()
        {
            if (Controller.TryTurnFinish())
            {
                return;
            }

            if (InputController.GetGameHotkeyUp(S.Input.CancelItem))
            {
                ChangeState(new PlayerState());
            }
        }

        public override void Exit()
        {
            if (ItemAbortable != null)
            {
                ItemAbortable.Abort();
            }
        }

        public override bool IsSaveable()
        {
            return Game.Loader != null;
        }

        void ItemSelectorStart()
        {
            //Create view for Item selector
            var data = new ItemAction(Battle.Player, Controller.Unit, Item);
            ItemAbortable = Item.Data.Selector.StartActivation(data, ItemSelectorFinish, Controller);
        }

        void ItemSelectorFinish(ItemAction action)
        {
            //Play created ItemAction
            ItemAbortable = null;
            ChangeState(new SpectatorState());
            Battle.TurnPlayer.Play(action, () => ChangeState(new PlayerState()));
        }
    }
}