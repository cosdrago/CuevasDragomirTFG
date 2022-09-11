using System.Linq;
using UnityEngine;

namespace RedBjorn.SuperTiles.Resolvers
{
    /// <summary>
    /// TurnResolver which trasit turn from unit with highest MoveRange value to lowest
    /// </summary>
    [CreateAssetMenu(menuName = Paths.ScriptablePath.Level.TurnResolver.MoveRange)]
    public class MoveRange : TurnResolver
    {
        public override bool CanChangeUnit => false;

        public override void TurnSequenceInit(BattleEntity battle)
        {
            var stat = S.Battle.Tags.Unit.MoveRange;
            battle.UnitsTimeline = battle.UnitsAlive.OrderByDescending(u => u.Stats.TryGetOrDefault(stat).Result)
                                                   .ThenBy(u =>
                                                           {
                                                               var player = battle.Players.FirstOrDefault(p => p.Squad.Contains(u));
                                                               if (player != null)
                                                               {
                                                                   return player.Id;
                                                               }
                                                               return 1000;
                                                           })
                                                   .ThenBy(u => u.Id)
                                                   .ToList();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Turn Unit sequence:");
            foreach (var unit in battle.UnitsTimeline)
            {
                var player = battle.Players.FirstOrDefault(p => p.Squad.Contains(unit));
                sb.Append($"{unit}: ");
                sb.Append($"Move range = {unit.Stats.TryGetOrDefault(stat).Result}. ");
                sb.AppendLine($"Player = {player}");
            }
            Log.I(sb.ToString());
        }

        public override void TurnSequenceStart(BattleEntity battle)
        {
            battle.TurnUnits.Clear();
            battle.TurnUnits.Add(battle.Unit);
        }

        public override void TurnSequenceFinish(BattleEntity battle)
        {
            var unit = battle.UnitsTimeline[0];
            battle.UnitsTimeline.RemoveAt(0);
            battle.UnitsTimeline.Add(unit);
        }
    }
}
