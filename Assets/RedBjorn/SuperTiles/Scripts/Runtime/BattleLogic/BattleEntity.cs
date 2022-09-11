﻿using RedBjorn.ProtoTiles;
using RedBjorn.SuperTiles.Battle;
using RedBjorn.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RedBjorn.SuperTiles
{
    /// <summary>
    /// Battle state
    /// </summary>
    [Serializable]
    public class BattleEntity
    {
        /// <summary>
        /// Current battle state
        /// </summary>
        public BattleState State;

        /// <summary>
        /// Current turn
        /// </summary>
        public int Turn;

        /// <summary>
        /// Current turn state
        /// </summary>
        public TurnState TurnState;

        public float TurnDuration;
        /// <summary>
        /// Selected player
        /// </summary>
        [SerializeReference]
        public SquadControllerEntity Player;

        /// <summary>
        /// All squad controllers at current battle 
        /// </summary>
        [SerializeReference]
        public List<SquadControllerEntity> Players = new List<SquadControllerEntity>();

        [SerializeReference]
        public List<SquadControllerEntity> Winners = new List<SquadControllerEntity>();

        /// <summary>
        /// Id to mark next registered unit
        /// </summary>
        public int UnitId;

        /// <summary>
        /// Selected unit
        /// </summary>
        [SerializeReference]
        public UnitEntity Unit;

        /// <summary>
        /// All alive units at current battle
        /// </summary>
        [SerializeReference]
        public List<UnitEntity> UnitsAlive = new List<UnitEntity>();

        /// <summary>
        /// All dead units at current battle
        /// </summary>
        [SerializeReference]
        public List<UnitEntity> UnitsDead = new List<UnitEntity>();

        /// <summary>
        /// All ai which control alive units
        /// </summary>
        [SerializeReference]
        public List<UnitAiEntity> UnitAis = new List<UnitAiEntity>();

        /// <summary>
        /// Order of units turns
        /// </summary>
        [SerializeReference]
        public List<UnitEntity> UnitsTimeline = new List<UnitEntity>();

        /// <summary>
        /// Units which can make actions during current turn
        /// </summary>
        [SerializeReference]
        public List<UnitEntity> TurnUnits = new List<UnitEntity>();

        /// <summary>
        /// Items which were used during current turn
        /// </summary>
        [SerializeReference]
        public List<ItemEntity> TurnItems = new List<ItemEntity>();

        /// <summary>
        /// Actions which are already completed during current turn
        /// </summary>
        [SerializeReference]
        public List<BaseAction> TurnActions = new List<BaseAction>();

        /// <summary>
        /// Selected level
        /// </summary>
        [NonSerialized]
        public LevelData Level;

        /// <summary>
        /// Current map state
        /// </summary>
        [NonSerialized]
        public MapEntity Map;

        /// <summary>
        /// Short access to game state
        /// </summary>
        [NonSerialized]
        public GameEntity Game;

        /// <summary>
        /// Player that handles IAction
        /// </summary>
        [NonSerialized]
        public ITurnPlayer TurnPlayer;

        public event Action OnTurnStarted;
        public event Action OnTurnFinishStarted;
        public event Action OnBattleFinished;

        public BattleEntity()
        {
            Turn = 0;
            TurnDuration = -1f;
            UnitId = 1;
            State = BattleState.None;
            TurnState = TurnState.None;
        }

        public void Destroy()
        {
            TurnPlayer.Destroy();
            Log.I("Battle destroyed");
        }

        public void RegisterUnit(UnitEntity unit)
        {
            UnitsAlive.Add(unit);
            UnitId++;
        }

        public void RegisterUnitAi(UnitAiEntity ai)
        {
            UnitAis.Add(ai);
        }

        /// <summary>
        /// Do preparation actions for battle start
        /// </summary>
        public void BattleStart()
        {
            if (State != BattleState.None)
            {
                Log.E("Battle already started");
                return;
            }

            State = BattleState.Started;
            Level.Turn.TurnSequenceInit(this);
            TurnPlayer.Start();
        }

        public void RaiseOnTurnStarted()
        {
            OnTurnStarted.SafeInvoke();
        }

        public void RaiseOnTurnFinishStarted()
        {
            OnTurnFinishStarted.SafeInvoke();
        }

        public void RaiseOnBattleFinished()
        {
            OnBattleFinished.SafeInvoke();
        }
    }
}

