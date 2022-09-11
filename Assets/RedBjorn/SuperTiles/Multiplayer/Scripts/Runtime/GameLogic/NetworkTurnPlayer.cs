using RedBjorn.SuperTiles.Battle;
using RedBjorn.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RedBjorn.SuperTiles.Multiplayer
{
    /// <summary>
    /// Class which implements the way actions are scheduled/played/sent over network
    /// </summary>
    public class NetworkTurnPlayer : ITurnPlayer, IOnBattleActionCallbacks, ITurnCallbacks
    {
        public class Data
        {
            public BaseAction Action;
            public Action OnCompleted;
        }

        public class SerializedData
        {
            public byte[] Action;
            public Action OnCompleted;
        }

        bool TurnFinishSent;
        bool FinishLaunchScheduled;
        bool FinishReceivedScheduled;
        double TurnTimeStart;
        BattleEntity Battle;
        Coroutine TurnStartCoroutine;
        Coroutine TurnStartNotifyCoroutine;
        Coroutine TurnFinishCoroutine;
        Coroutine TurnDurationVerifyCoroutine;
        Coroutine TurnFinishCompletedVerifyCoroutine;
        List<SquadControllerEntity> LocalPlayers = new List<SquadControllerEntity>();
        List<Data> LocalScheduled = new List<Data>();
        List<Data> LocalInProgress = new List<Data>();
        List<SerializedData> Sent = new List<SerializedData>();
        List<Data> ReceivedScheduled = new List<Data>();
        List<Data> ReceivedInProgress = new List<Data>();

        /// <summary>
        /// Players which have finished turn finishin process
        /// </summary>
        HashSet<INetworkPlayer> PlayersFinishTurn = new HashSet<INetworkPlayer>();

        bool IsPlaying
        {
            get
            {
                return LocalScheduled.Count > 0
                        || LocalInProgress.Count > 0
                        || Sent.Count > 0
                        || ReceivedScheduled.Count > 0
                        || ReceivedInProgress.Count > 0;
            }
        }

        public NetworkSettings Network => S.Network;

        /// <summary>
        /// Time which have passed from the start of a turn
        /// </summary>
        public float TimePassed => (float)(NetworkController.Time - TurnTimeStart);

        public NetworkTurnPlayer(BattleEntity battle)
        {
            Battle = battle;
            Battle.TurnDuration = NetworkController.GetCurrentRoom().TurnDuration;
            NetworkController.AddCallbackTarget(this);
        }

        /// <summary>
        /// Start battle
        /// </summary>
        public void Start()
        {
            NetworkController.SendBattleStart();
        }

        public void Destroy()
        {
            TurnStartAbort();
            TurnStartNotifyAbort();
            TurnFinishAbort();
            TurnDurationVerifyAbort();
            TurnFinishCompletedAbort();
            NetworkController.RemoveCallbackTarget(this);
        }

        public void LocalPlayerAdd(SquadControllerEntity player)
        {
            LocalPlayers.Add(player);
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

            TurnStartCoroutine = CoroutineLauncher.Launch(TurnStarting(), TurnStartActions);
        }

        void TurnStartAbort()
        {
            if (TurnStartCoroutine != null)
            {
                CoroutineLauncher.Finish(TurnStartCoroutine);
                Log.I("Turn start aborted");
                TurnStartCoroutine = null;
            }
        }

        /// <summary>
        /// Do preparations at turn start
        /// </summary>
        IEnumerator TurnStarting()
        {
            Battle.TurnState = TurnState.Starting;
            Battle.Turn++;
            Log.I($"Turn: {Battle.Turn.ToString()} {Battle.TurnState}");
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
            TurnStartCoroutine = null;
        }

        /// <summary>
        /// Finish preparation actions at turn start
        /// </summary>
        void TurnStartActions()
        {
            TurnTimeStart = NetworkController.Time;
            Battle.TurnState = TurnState.Started;
            Log.I($"Turn: {Battle.Turn.ToString()} {Battle.TurnState}");
            TurnStartNotifyCoroutine =  CoroutineLauncher.Launch(TurnStartNotify());
        }

        /// <summary>
        /// Raise event that turn start process is completed
        /// </summary>
        /// <returns></returns>
        IEnumerator TurnStartNotify()
        {
            yield return null;
            TurnDurationVerifyLaunch();
            Battle.RaiseOnTurnStarted();
            TurnStartNotifyCoroutine = null;
        }

        void TurnStartNotifyAbort()
        {
            if (TurnStartNotifyCoroutine != null)
            {
                CoroutineLauncher.Finish(TurnStartNotifyCoroutine);
                Log.I("Turn start notify aborted");
                TurnStartNotifyCoroutine = null;
            }
        }

        /// <summary>
        /// Play battle action
        /// </summary>
        /// <param name="action"></param>
        /// <param name="onCompleted"></param>
        public void Play(BaseAction action, Action onCompleted)
        {
            LocalScheduled.Add(new Data { Action = action, OnCompleted = onCompleted });
            PlayTryLocal();
        }

        /// <summary>
        /// Try to play scheduled actions which could be local or network
        /// </summary>
        void PlayTryLocal()
        {
            if (LocalScheduled.Count == 0)
            {
                Log.I("Nothing to play");
                if (FinishLaunchScheduled)
                {
                    FinishLaunchScheduled = false;
                    Log.I("Launch turn finish because it was scheduled");
                    TurnFinishLaunch();
                } 
                else if (FinishReceivedScheduled)
                {
                    FinishReceivedScheduled = false;
                    Log.I("Play turn finish because it was scheduled");
                    TurnFinishPlay();
                }
                return;
            }

            if (LocalInProgress.Count > 0)
            {
                Log.W("Won't play action. Cause some actions are in progress");
                return;
            }

            var next = LocalScheduled[0];
            LocalScheduled.RemoveAt(0);

            if (!Battle.Level.Actions.Validate(next.Action, Battle))
            {
                Log.E($"Won't play action. Action is invalid. {next.Action}");
                next.OnCompleted.SafeInvoke();
                PlayTryLocal();
                return;
            }

            if (LocalPlayers.Contains(next.Action.Player))
            {
                LocalInProgress.Add(next);
                next.Action.Do(() =>
                {
                    LocalInProgress.Remove(next);
                    Battle.TurnActions.Add(next.Action);
                    next.OnCompleted.SafeInvoke();
                    PlayTryLocal();
                },
                Battle);
            }
            else
            {
                var serializedAction = next.Action.Serialize();
                Sent.Add(new SerializedData { Action = serializedAction, OnCompleted = next.OnCompleted });
                Log.I($"Send battle action: {next.Action.ToString()}");
                NetworkController.SendBattleAction(serializedAction);
            }
        }

        /// <summary>
        /// Try to play received actions
        /// </summary>
        void PlayTryReceived()
        {
            if (ReceivedScheduled.Count == 0)
            {
                Log.I("Nothing to play");
                if (FinishLaunchScheduled)
                {
                    FinishLaunchScheduled = false;
                    Log.I("Launch turn finish because it was scheduled");
                    TurnFinishLaunch();
                }
                else if (FinishReceivedScheduled)
                {
                    FinishReceivedScheduled = false;
                    Log.I("Play turn finish because it was scheduled");
                    TurnFinishPlay();
                }
                return;
            }

            if (ReceivedInProgress.Count > 0)
            {
                Log.W("Won't play action. Cause some actions are in progress");
                return;
            }

            var next = ReceivedScheduled[0];
            ReceivedScheduled.RemoveAt(0);

            if (next.Action.Player != Battle.Player)
            {
                Log.E($"Won't play action: {next.Action.ToString()}. Incorrect Player");
                return;
            }

            if (!Battle.Level.Actions.Validate(next.Action, Battle))
            {
                Log.E("Won't play action. Data is invalid");
                var completed = next.OnCompleted;
                next.OnCompleted = null;
                completed.SafeInvoke();
                return;
            }

            ReceivedInProgress.Add(next);
            next.Action.Do(() =>
            {
                ReceivedInProgress.Remove(next);
                Battle.TurnActions.Add(next.Action);
                next.OnCompleted.SafeInvoke();
                PlayTryReceived();
            },
            Battle);
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
                Log.W("Can't launch finish turn. Turn is playing. Launch scheduled");
                FinishLaunchScheduled = true;
                return;
            }

            if (FinishLaunchScheduled)
            {
                Log.W("Can't launch finish turn. Launch already scheduled");
                return;
            }

            TurnFinishLaunch();
        }

        void TurnFinishLaunch()
        {
            if (LocalPlayers.Contains(Battle.Player))
            {
                TurnFinishPlay();
            }
            else if (!TurnFinishSent)
            {
                TurnFinishSent = true;
                NetworkController.SendTurnFinishLaunch(Battle.Turn);
            }
            else
            {
                Log.W("TurnFinish has already sent");
            }
        }

        /// <summary>
        /// Launch turn finishing process
        /// </summary>
        void TurnFinishPlay()
        {
            TurnFinishCoroutine = CoroutineLauncher.Launch(TurnFinishing(), TurnFinishActions);
        }

        void TurnFinishAbort()
        {
            if (TurnFinishCoroutine != null)
            {
                CoroutineLauncher.Finish(TurnFinishCoroutine);
                Log.I("Turn finish aborted");
                TurnFinishCoroutine = null;
            }
        }

        /// <summary>
        /// Do preparation actions at turn finish
        /// </summary>
        IEnumerator TurnFinishing()
        {
            Battle.RaiseOnTurnFinishStarted();
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
            if (Battle.State == BattleState.Finished)
            {
                TurnDurationVerifyAbort();
                TurnFinishCompletedAbort();
                Battle.RaiseOnBattleFinished();
            }
            else
            {
                NetworkController.SendTurnFinishCompleted(Battle.Turn);
            }
        }

        void IOnBattleActionCallbacks.OnBattleAction(byte[] data)
        {
            var existed = Sent.FirstOrDefault(s => s.Action.SequenceEqual(data));
            if (existed != null)
            {
                Sent.Remove(existed);
            }

            var dataAction = new Data
            {
                Action = BaseAction.Deserialize(data, Battle),
                OnCompleted = existed == null ? null : existed.OnCompleted
            };
            Log.I($"Received battle action {dataAction.Action.ToString()}");
            ReceivedScheduled.Add(dataAction);
            PlayTryReceived();
        }

        void ITurnCallbacks.OnTurnStart(double timeStart)
        {
            if (Battle.State != BattleState.Finished)
            {
                TurnTimeStart = timeStart;
                TurnStart();
            }
        }

        void ITurnCallbacks.OnTurnFinishLaunched(int turn)
        {
            TurnFinishSent = false;
            TurnDurationVerifyAbort();
            TurnFinishCompletedLaunch();

            if (Battle.State != BattleState.Started)
            {
                NetworkController.Error($"Can't start turn. Battle state: {Battle.State}");
                return;
            }

            if (Battle.TurnState != TurnState.Started)
            {
                NetworkController.Error("Can't finish turn. Turn didn't start");
                return;
            }

            if (Battle.Turn != turn)
            {
                NetworkController.Error("Can't finish turn. Invalid turn request");
                return;
            }

            if (IsPlaying)
            {
                Log.W("Can't finish turn. Turn is playing. Play turn finish scheduled");
                FinishReceivedScheduled = true;
                return;
            }

            if (FinishReceivedScheduled)
            {
                Log.W("Can't finish turn. Already scheduled");
                return;
            }

            TurnFinishPlay();
        }

        void ITurnCallbacks.OnTurnFinishCompleted(INetworkPlayer player, int turn)
        {
            if (player == null || !PlayersFinishTurn.Add(player))
            {
                NetworkController.Error("Unexpected behaviour on OnTurnFinishCompleted");
                return;
            }
            Log.I($"Received TurnFinishCompleted from Player: {player.Nickname}. Turn: {turn}");
            if (PlayersFinishTurn.Count == NetworkController.GetCurrentRoom().Players.Count)
            {
                TurnFinishCompletedAbort();
                PlayersFinishTurn.Clear();
                NetworkController.SendBattleStart();
            }
        }

        /// <summary>
        /// Launch verification that player will finish its turn
        /// </summary>
        void TurnDurationVerifyLaunch()
        {
            if (NetworkController.IsMaster())
            {
                TurnDurationVerifyAbort();
                TurnDurationVerifyCoroutine = CoroutineLauncher.Launch(TurnDurationVerifying());
            }
        }

        void TurnDurationVerifyAbort()
        {
            if (TurnDurationVerifyCoroutine != null)
            {
                CoroutineLauncher.Finish(TurnDurationVerifyCoroutine);
                Log.I("Turn duration verify aborted");
                TurnDurationVerifyCoroutine = null;
            }
        }

        IEnumerator TurnDurationVerifying()
        {
            if (Battle.TurnDuration > 0f)
            {
                Log.I("Turn duration verify launched");
                yield return new WaitForSecondsRealtime(Battle.TurnDuration + Network.TurnDurationVerifyDelay);
                Log.W("Don't receive complete turn message. Will send it");
                NetworkController.SendTurnFinishLaunch(Battle.Turn);
            }
            TurnDurationVerifyCoroutine = null;
        }

        /// <summary>
        /// Lauch verification that all players will complete turn finishing process
        /// </summary>
        void TurnFinishCompletedLaunch()
        {
            if (NetworkController.IsMaster())
            {
                TurnFinishCompletedAbort();
                TurnFinishCompletedVerifyCoroutine = CoroutineLauncher.Launch(TurnFinishCompletedVerifying());
            }
        }

        void TurnFinishCompletedAbort()
        {
            if (TurnFinishCompletedVerifyCoroutine != null)
            {
                CoroutineLauncher.Finish(TurnFinishCompletedVerifyCoroutine);
                Log.I("Turn finish completed verify aborted");
                TurnFinishCompletedVerifyCoroutine = null;
            }
        }

        IEnumerator TurnFinishCompletedVerifying()
        {
            Log.I("Turn finish completed verify launched");
            yield return new WaitForSecondsRealtime(Network.TurnFinishCompletedVerifyDelay);
            var players = NetworkController.GetCurrentRoom().Players.Except(PlayersFinishTurn).ToArray();
            NetworkController.Error($"These players didn't completed finish turn\n{string.Join(", ", players.Select(s => s.Nickname))}");
            TurnFinishCompletedVerifyCoroutine = null;
        }
    }
}
