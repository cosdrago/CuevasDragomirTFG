using RedBjorn.SuperTiles.UI;
using RedBjorn.Utils;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace RedBjorn.SuperTiles.Battle.States
{
    /// <summary>
    /// BattleView state which is a initial state for Player loop
    /// </summary>
    public class BattleFinishState : State
    {
        protected override void Enter()
        {
            CoroutineLauncher.Launch(Finishing());
        }

        public override void Update()
        {

        }

        public override void Exit()
        {

        }

        public override void OnUnitChanged()
        {

        }

        public override void OnTurnStarted()
        {

        }

        public override void OnTurnFinishStarted()
        {

        }

        public override void OnBattleFinish()
        {

        }

        IEnumerator Finishing()
        {
            Controller.Status = Controller.Statuses.OnBattleFinish;
            BattleFinishUI.Show();
            yield return new WaitForSecondsRealtime(3f);
            ConfirmMessageUI.Show($"{string.Join(", ", Battle.Winners.Select(w => w.Nickname))} win!\nGo to menu?",
                "Yes",
                "No",
                () => { SceneLoader.Load(S.Levels.MenuSceneName); },
                null);
        }
    }
}