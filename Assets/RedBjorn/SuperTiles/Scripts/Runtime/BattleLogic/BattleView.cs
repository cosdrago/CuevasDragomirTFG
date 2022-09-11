﻿using RedBjorn.ProtoTiles;
using RedBjorn.SuperTiles.Battle;
using RedBjorn.SuperTiles.Battle.States;
using RedBjorn.SuperTiles.UI;
using RedBjorn.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedBjorn.SuperTiles
{
    /// <summary>
    /// View which represents BattleEntity at unity scene and manipulates Battle states
    /// </summary>
    public class BattleView : MonoBehaviour
    {
        public string Status { get; set; }
        public bool Paused { get; private set; }
        public State State { get; private set; }
        public Settings.BattleSettings.StatusSettings Statuses { get; private set; }
        public InteractableDetector Detector { get; private set; }
        public List<SquadControllerEntity> Owners { get; private set; }
        public GameEntity Game { get; private set; }
        public BattleEntity Battle { get { return Game.Battle; } }
        public MapEntity Map { get { return Battle.Map; } }
        public UnitEntity Unit { get { return Battle.Unit; } }
        public bool IsMyPlayer { get { return Owners.Contains(Battle.Player); } }

        MenuGameUI MenuUI;
        Coroutine TurnAutoCompleteCoroutine;
        const string SaveSlot = "1";

        public ItemsPanelUI CachedItemsUI;
        public ItemsPanelUI ItemsUI
        {
            get
            {
                if (!CachedItemsUI)
                {
                    CachedItemsUI = FindObjectOfType<ItemsPanelUI>();
                }
                return CachedItemsUI;
            }
        }

        public TeamPanelUI CachedTeamPanel;
        public TeamPanelUI TeamPanelUI
        {
            get
            {
                if (!CachedTeamPanel)
                {
                    CachedTeamPanel = FindObjectOfType<TeamPanelUI>();
                }
                return CachedTeamPanel;
            }
        }

        void Start()
        {
            Statuses = S.Battle.Status;
            Status = Statuses.OnStart;
            //Define initial states
            ChangeState(new IdleState());

            //Cache essentials
            var menuButtonUI = FindObjectOfType<ClosePanelUI>();
            if (menuButtonUI)
            {
                menuButtonUI.Init(DoShowMenuGame);
            }
            MenuUI = FindObjectOfType<MenuGameUI>();
            if (MenuUI)
            {
                MenuUI.Init(DoRestart, DoSave, DoLoad, DoMenuMain, this);
            }
            var statusPanel = FindObjectOfType<StatusPanelUI>();
            if (statusPanel)
            {
                statusPanel.Init(this, S.Battle.UI.StatusTextShow);
            }
            Detector = FindObjectOfType<InteractableDetector>();
        }

        void Update()
        {
            if (Paused || Game == null || Battle == null)
            {
                return;
            }
            Input(); //Get input
            State.Update(); //Update current state
        }

        void OnDestroy()
        {
            if (Game != null)
            {
                Game.OnStarted -= OnBattleCreated;
                if (Game.Battle != null)
                {
                    Game.Battle.OnTurnStarted -= OnTurnStarted;
                    Game.Battle.OnTurnFinishStarted -= OnTurnFinishStarted;
                    Game.Battle.OnBattleFinished -= OnBattleFinished;
                }
            }
        }

        public void Init(GameEntity game, List<SquadControllerEntity> owners)
        {
            Game = game;
            Owners = owners;
            Game.Battle.OnTurnStarted += OnTurnStarted;
            Game.Battle.OnTurnFinishStarted += OnTurnFinishStarted;
            Game.Battle.OnBattleFinished += OnBattleFinished;
            //If AutoStart than wait for BattleCreation and change BattleView state to BattleStartState
            //If not than wait when battle start will be invoked by clicking Start button
            var battleStart = FindObjectOfType<BattleStartUI>();
            if (Battle.Level.AutoStart)
            {
                if (battleStart)
                {
                    battleStart.Init(null);
                }
                if (Game != null)
                {
                    Game.OnStarted += OnBattleCreated;
                }
            }
            else
            {
                if (battleStart)
                {
                    battleStart.Init(BattleStart);
                }
            }
        }

        public void ChangeState(State state)
        {
            Log.I($"State exit: {State}");
            if (State != null)
            {
                State.Exit();
            }
            State = state;
            Log.I($"State enter: {State}");
            if (State != null)
            {
                State.OnEntered(this);
            }
        }

        public void Pause()
        {
            Paused = true;
        }

        public void Unpause()
        {
            Paused = false;
        }

        public void BattleStart()
        {
            Unpause();
            var turnUI = FindObjectOfType<TurnPanelUI>();
            if (turnUI)
            {
                turnUI.Init(this, FinishTurnAsk, S.Battle.UI.TurnShow);
            }
            var turnDurationUI = FindObjectOfType<TurnDurationUI>();
            if (turnDurationUI)
            {
                turnDurationUI.Init(this, S.Battle.UI.TurnShow);
            }
            Status = Statuses.OnBattleStart;
            ChangeState(new SpectatorState());
            Log.I($"Battle state initial: {Battle.State}");
            switch (Battle.State)
            {
                case BattleState.None: Game.Battle.BattleStart(); break;
                case BattleState.Started: OnBattleStarted(); break;
                case BattleState.Finished: break;
            }
        }

        /// <summary>
        /// Try to complete current turn
        /// </summary>
        /// <returns>true if input invoked, false otherwise</returns>
        public bool TryTurnFinish()
        {
            if (InputController.GetGameHotkeyUp(S.Input.CompleteTurn))
            {
                FinishTurnAsk();
                return true;
            }
            return false; ;
        }

        public bool TryUnitSelect(UnitEntity unit)
        {
            if (Battle.Level.Turn.CanChangeUnit && Unit != unit && Battle.Player.Squad.Contains(unit))
            {
                Battle.Unit = unit;
                State.OnUnitChanged();
                return true;
            }
            return false;
        }

        public bool TryUnitSelect(Vector3 position)
        {
            var tile = Map.Tile(position);
            if (tile != null && tile.HasUnit)
            {
                return TryUnitSelect(tile.Unit);
            }
            return false;
        }

        void OnBattleCreated()
        {
            if (Game != null)
            {
                Game.OnStarted -= OnBattleCreated;
            }
            BattleStart();
        }

        void OnBattleStarted()
        {
            switch (Battle.TurnState)
            {
                case TurnState.None: break;
                case TurnState.Started: OnTurnStarted(); break;
                case TurnState.Finished: Battle.TurnPlayer.TurnStart(); break;
                case TurnState.Starting:
                case TurnState.Finishing:
                    Log.E("Invalid battle turn state to start battle.");
                    break;
            }
        }

        void OnTurnStarted()
        {
            TurnAutoCompleteLaunch();
            State.OnTurnStarted();
        }

        void OnTurnFinishStarted()
        {
            TurnAutoCompleteAbort();
            State.OnTurnFinishStarted();
        }

        void OnBattleFinished()
        {
            TurnAutoCompleteAbort();
            State.OnBattleFinish();
        }

        void Input()
        {
            if (InputController.GetGameHotkeyUp(S.Input.Menu))
            {
                DoShowMenuGame();
            }
        }

        void DoShowMenuGame()
        {
            if (MenuUI)
            {
                MenuUI.Show();
            }
        }

        void DoRestart()
        {
            if (State.IsRestartable())
            {
                GameEntity.Current = new GameEntity()
                {
                    Creator = Game.Creator,
                    Loader = Game.Loader,
                    Restartable = Game.Restartable,
                    Level = Game.Level
                };
                SceneLoader.Load(Game.Level.SceneName, S.Levels.GameSceneName);
            }
            else
            {
                Log.E("Can't restart. Invalid battle state");
            }
        }

        void DoSave()
        {
            if (State.IsSaveable())
            {
                SaveController.SaveGame(Game, SaveSlot);
            }
            else
            {
                Log.E("Can't save. Invalid battle state");
            }
        }

        void DoLoad()
        {
            if (State.IsLoadable())
            {
                SaveController.LoadGame(SaveSlot, (save) =>
                {
                    GameEntity.Current = save.State;
                    SceneLoader.Load(save.State.Level.SceneName, S.Levels.GameSceneName);
                });
            }
            else
            {
                Log.E("Can't load. Invalid battle state");
            }
        }

        public void DoMenuMain()
        {
            SceneLoader.Load(S.Levels.MenuSceneName);
        }

        void FinishTurnAsk()
        {
            if (Battle != null && Battle.Turn > 0 && IsMyPlayer && Battle.State == BattleState.Started)
            {
                Pause();
                ConfirmMessageUI.Show("Complete turn?", "Yes", "No",
                () =>
                {
                    Unpause();
                    FinishTurnDo();
                },
                () =>
                {
                    Unpause();
                });
            }
        }

        void FinishTurnDo()
        {
            if (IsMyPlayer)
            {
                Status = Statuses.TurnFinish;
                ChangeState(new SpectatorState());
                Battle.TurnPlayer.TurnFinish();
            }
        }

        public void TurnAutoCompleteLaunch()
        {
            if (!IsMyPlayer)
            {
                return;
            }
            TurnAutoCompleteAbort();
            TurnAutoCompleteCoroutine = CoroutineLauncher.Launch(TurnAutoCompleting());
        }

        void TurnAutoCompleteAbort()
        {
            if (TurnAutoCompleteCoroutine != null)
            {
                CoroutineLauncher.Finish(TurnAutoCompleteCoroutine);
                TurnAutoCompleteCoroutine = null;
                Log.I($"Turn auto-completing aborted");
            }
        }

        IEnumerator TurnAutoCompleting()
        {
            Log.I($"Turn auto-completing launched");
            if (Battle.TurnDuration > 0f)
            {
                yield return new WaitForSecondsRealtime(Battle.TurnDuration);
                Log.I($"Turn auto-completing fired");
                Battle.TurnPlayer.TurnFinish();
            }
            TurnAutoCompleteCoroutine = null;
        }
    }
}
