using RedBjorn.SuperTiles.Battle;
using RedBjorn.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RedBjorn.SuperTiles
{
    /// <summary>
    /// Class which implements the way actions are scheduled/played
    /// </summary>
    public class TurnPlayer : ITurnPlayer
    {
        public class Data
        {
            public BaseAction Action;
            public Action OnCompleted;
        }

        DateTime TimeStart;
        BattleEntity Battle;
        List<Data> Scheduled = new List<Data>();
        List<Data> InProgress = new List<Data>();

        bool IsPlaying { get { return InProgress.Count > 0 || Scheduled.Count > 0; } }

        public float TimePassed
        {
            get
            {
                var delta = DateTime.UtcNow.Ticks - TimeStart.Ticks;
                return (float)(new TimeSpan(delta).TotalSeconds);
            }
        }

        public TurnPlayer(BattleEntity battle)
        {
            Battle = battle;
        }

        /// <summary>
        /// Do main loop actions at turn start
        /// </summary>
        public void TurnStart()
        {
            if (Battle.State != BattleState.Started)
            {
                Log.E($"Can't start turn. Battle state: {Battle.State}");
                return;
            }

            if (Battle.TurnState != TurnState.Finished && Battle.TurnState != TurnState.None)
            {
                Log.E("Can't start turn. Turn didn't finish");
                return;
            }

            if (Battle.UnitsTimeline.Count == 0)
            {
                Log.E("Can't start turn. Empty unit timeline sequence");
                return;
            }

            CoroutineLauncher.Launch(TurnStarting(), TurnStartActions);
        }

        /// <summary>
        /// Do preparations at turn start
        /// </summary>
        IEnumerator TurnStarting()
        {
            Battle.TurnState = TurnState.Starting;
            Battle.Turn++;
            Battle.Unit = Battle.UnitsTimeline[0]; //Select default unit
            Battle.Player = Battle.Players.FirstOrDefault(p => p.Squad.Contains(Battle.Unit)); //Select new player
            Battle.Level.Turn.TurnSequenceStart(Battle);
            foreach (var unit in Battle.TurnUnits)
            {
                yield return unit.OnStartTurn(Battle);
                foreach (var ai in Battle.UnitAis.Where(a => a.Unit == unit))
                {
                    ai.OnStartTurn();
                }
            }
        }

        /// <summary>
        /// Finish preparation actions at turn start
        /// </summary>
        void TurnStartActions()
        {
            TimeStart = DateTime.UtcNow;
            Battle.TurnState = TurnState.Started;
            Log.I($"Turn: {Battle.Turn.ToString()} {Battle.TurnState}");
            Battle.RaiseOnTurnStarted();
        }

        public void Play(BaseAction action, Action onCompleted)
        {
            Scheduled.Add(new Data { Action = action, OnCompleted = onCompleted });
            PlayTry();
        }

        void PlayTry()
        {
            if (Scheduled.Count == 0)
            {
                return;
            }

            if (InProgress.Count > 0)
            {
                Log.I("Try play. Already in progress");
                return;
            }

            var next = Scheduled[0];
            Scheduled.RemoveAt(0);
            if (next.Action == null)
            {
                Log.E("Action won't play. Action Null");
                next.OnCompleted.SafeInvoke();
                PlayTry();
            }
            else if (Battle.Level.Actions.Validate(next.Action, Battle))
            {
                InProgress.Add(next);
                next.Action.Do(() =>
                {
                    InProgress.Remove(next);
                    Battle.TurnActions.Add(next.Action);
                    next.OnCompleted.SafeInvoke();
                    PlayTry();
                },
                Battle);
            }
            else
            {
                Log.E($"Action won't play. Action is invalid. {next.Action}");
                next.OnCompleted.SafeInvoke();
                PlayTry();
            }
        }

        /// <summary>
        /// Do main loop actions at turn finish
        /// </summary>
        public void TurnFinish()
        {
            if (Battle.State != BattleState.Started)
            {
                Log.E($"Can't start turn. Battle state: {Battle.State}");
                return;
            }

            if (Battle.TurnState != TurnState.Started)
            {
                Log.E("Can't finish turn. Turn didn't start");
                return;
            }

            if (IsPlaying)
            {
                Log.E("Can't finish turn. Turn is playing");
                return;
            }

            CoroutineLauncher.Launch(TurnFinishing(), TurnFinishActions);
        }

        /// <summary>
        /// Do preparation actions at turn finish
        /// </summary>
        IEnumerator TurnFinishing()
        {
            Battle.TurnState = TurnState.Finishing;
            //Handle end turn action for Units and Ai
            foreach (var unit in Battle.TurnUnits)
            {
                yield return unit.OnFinishTurn(Battle);
                foreach (var ai in Battle.UnitAis.Where(a => a.Unit == unit))
                {
                    ai.OnFinishTurn();
                }
            }
        }

        /// <summary>
        /// Finish preparation actions at turn finish
        /// </summary>
        void TurnFinishActions()
        {
            Battle.TurnActions.Clear();
            Battle.TurnItems.Clear();

            //Update Alive and Dead unit list
            for (int i = Battle.UnitsAlive.Count - 1; i >= 0; i--)
            {
                var unit = Battle.UnitsAlive[i];
                if (unit.IsDead)
                {
                    Battle.UnitsTimeline.Remove(unit);
                    Battle.UnitsAlive.RemoveAt(i);
                    Battle.UnitsDead.Add(unit);
                }
            }

            //Update map
            Battle.Map.Clear();
            foreach (var u in Battle.UnitsAlive)
            {
                Battle.Map.RegisterUnit(u);
            }

            //Check battle finish
            Battle.Level.BattleFinish.Handle(Battle);
            //Calculate units for next turn
            Battle.Level.Turn.TurnSequenceFinish(Battle);
            Battle.TurnState = TurnState.Finished;
            Log.I($"Turn: {Battle.Turn.ToString()} {Battle.TurnState}");
            if (S.Battle.AutoSave)
            {
                SaveController.SaveGame(Battle.Game, "1", TurnOnFinished);
            }
            else
            {
                TurnOnFinished();
            }
        }

        void TurnOnFinished()
        {
            if (Battle.State == BattleState.Finished)
            {
                Battle.RaiseOnBattleFinished();
            }
            else
            {
                TurnStart();
            }
        }

        public void Start()
        {
            TurnStart();
        }

        public void Destroy()
        {

        }
    }
}
