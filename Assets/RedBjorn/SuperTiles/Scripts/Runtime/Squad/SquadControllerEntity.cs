using System;
using System.Collections.Generic;
using UnityEngine;

namespace RedBjorn.SuperTiles
{
    /// <summary>
    /// Entity which contains a list of units
    /// </summary>
    [Serializable]
    public abstract class SquadControllerEntity
    {
        public int Id;
        public string Nickname;
        public TeamTag Team;
        [SerializeReference]
        public List<UnitEntity> Squad;
        [NonSerialized]
        protected GameEntity Game;

        public SquadControllerEntity(int id, TeamTag team, GameEntity game)
        {
            Id = id;
            Team = team;
            Game = game;
            Squad = new List<UnitEntity>();
        }

        public void Load(GameEntity game)
        {
            Game = game;
        }

        public void AddUnit(UnitEntity unit)
        {
            Squad.Add(unit);
        }

        public virtual void OnMyTurnstarted() { }
    }
}