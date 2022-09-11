using RedBjorn.ProtoTiles;
using RedBjorn.SuperTiles.Battle;
using RedBjorn.SuperTiles.Battle.Actions;
using RedBjorn.SuperTiles.Squad;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RedBjorn.SuperTiles.Ai
{
    [CreateAssetMenu(menuName = Paths.ScriptablePath.Ai.Simple)]
    public class Simple : UnitAiData
    {
        [SerializeField]
        bool StopIfEnemyReachable;

        IEnumerable<UnitEntity> Enemies(BattleEntity battle, UnitEntity unit)
        {
            foreach (var player in battle.Players.Where(p => !p.Squad.Contains(unit)))
            {
                foreach (var u in player.Squad)
                {
                    yield return u;
                }
            }
        }

        bool GetMoveAction(AiEntity player, UnitAiEntity ai, BattleEntity battle, out BaseAction action)
        {
            action = null;
            UnitEntity target = null;
            IEnumerable<UnitEntity> targets = Enemies(battle, ai.Unit).Where(p => !p.IsDead).ToList();
            if (StopIfEnemyReachable && ai.Unit.Items.Count > 0)
            {
                var item = ai.Unit.Items[0];

                targets = item.PossibleTargets(ai.Unit.WorldPosition, battle)
                              .Where(p => targets.Contains(p));
                foreach (var enemy in targets.OrderBy(p => battle.Map.Distance(p.WorldPosition, ai.Unit.WorldPosition)))
                {
                    var reachable = item.Data.Selector.SelectTargets(item, ai.Unit.WorldPosition, enemy.WorldPosition, ai.Unit, battle);
                    if (reachable.Contains(enemy))
                    {
                        return false;
                    }
                }
            }
            target = battle.Map.NearestUnit(ai.Unit.WorldPosition, targets);
            if (target == null)
            {
                return false;
            }
            var point = Vector3Int.zero;
            Func<TileEntity, bool> condition = (t) => t.Vacant || t.Unit == ai.Unit;
            Func<Vector3Int, float> orderBy = (v) => battle.Map.Distance(target.TilePosition + v, ai.Unit.TilePosition);
            if (battle.Map.NearestPosition(target.TilePosition, out point, condition, orderBy))
            {
                if (point == ai.Unit.TilePosition)
                {
                    return false;
                }
                action = new MoveAction(player, ai.Unit, battle.Map.WorldPosition(point));
                return true;
            }

            return false;
        }

        bool GetItemAction(AiEntity player, UnitAiEntity ai, BattleEntity battle, out BaseAction action)
        {
            action = null;
            if (ai.Unit.Items.Count == 0)
            {
                Log.W($"Ai {ai.ToString()} has no items. Skip ItemAction");
                return false;
            }

            var item = ai.Unit.Items[0];
            var enemies = Enemies(battle, ai.Unit).ToList();
            var possible = item.PossibleTargets(ai.Unit.WorldPosition, battle)
                               .Where(p => !p.IsDead)
                               .Where(p => enemies.Contains(p))
                               .OrderBy(p => battle.Map.Distance(p.WorldPosition, ai.Unit.WorldPosition));

            UnitEntity enemy = null;
            foreach (var target in possible)
            {
                var reachable = item.Data.Selector.SelectTargets(item, ai.Unit.WorldPosition, target.WorldPosition, ai.Unit, battle);
                if (reachable.Contains(target))
                {
                    enemy = target;
                    break;
                }
            }
            if (enemy == null)
            {
                return false;
            }

            action = new ItemAction(player, ai.Unit, item, enemy.WorldPosition);
            return true;
        }

        public override bool TryNextAction(AiEntity player, UnitAiEntity ai, BattleEntity battle, out BaseAction action)
        {
            if (ai.TurnActionCount > 1)
            {
                action = null;
                return false;
            }

            if (ai.TurnActionCount == 0)
            {
                ai.UpdateCount();
                if (GetMoveAction(player, ai, battle, out action))
                {
                    return true;
                }
            }
            ai.UpdateCount();
            return GetItemAction(player, ai, battle, out action);
        }
    }
}