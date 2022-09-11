﻿using RedBjorn.SuperTiles.Battle.Actions;
using RedBjorn.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RedBjorn.SuperTiles
{
    /// <summary>
    /// Item state
    /// </summary>
    [Serializable]
    public class ItemEntity : UI.ITooltip
    {
        [Serializable]
        public class ItemStatDictionary : SerializableDictionary<ItemStatTag, StatEntity> { } // Hack to serialize dictionary

        public bool Used;
        public int CurrentCooldown;
        public int CurrentStackCount;
        public ItemData Data;
        public ItemStatDictionary Stats;

        public StatEntity this[ItemStatTag stat] { get { return Stats.TryGetOrDefault(stat); } }

        ItemEntity() { }

        public ItemEntity(ItemData data)
        {
            Data = data;
            CurrentCooldown = 0;
            CurrentStackCount = Data.MaxStackCount;
            Stats = new ItemStatDictionary();
            foreach (var s in Data.Stats)
            {
                Stats[s.Stat] = new StatEntity(s.Value);
            }
        }

        public void Load()
        {

        }

        public IEnumerable<UnitEntity> PossibleTargets(Vector3 attackPosition, BattleEntity battle)
        {
            return Data.Selector.PossibleTargets(this, attackPosition, battle);
        }

        public bool CanUse()
        {
            return !Used && CurrentCooldown == 0 && (!Data.Stackable || CurrentStackCount > 0f);
        }

        public void Use(ItemAction action, BattleEntity battle, Action onCompleted)
        {
            if (CanUse())
            {
                Used = true;
                if (Data.Stackable)
                {
                    CurrentStackCount = Mathf.Max(0, CurrentStackCount - 1);
                }
                CurrentCooldown = this[S.Battle.Tags.Item.Cooldown];
                CoroutineLauncher.Launch(Data.ActionHandler.Handle(action, battle), onCompleted);
            }
            else
            {
                string actionInfo = "Null";
                if (action != null)
                {
                    actionInfo = action.ToString();
                }
                Log.W($"Can't use item for action: {actionInfo}");
                onCompleted.SafeInvoke();
            }
        }

        public void OnFinishTurn()
        {
            HandleCooldown();
        }

        void HandleCooldown()
        {
            if (Used)
            {
                Used = false;
            }
            else
            {
                CurrentCooldown = Mathf.Max(0, --CurrentCooldown);
            }
        }

        public override string ToString()
        {
            return Data == null ? null : Data.name;
        }

        public string Text()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"{ToString()} \n");
            foreach (var kv in Stats)
            {
                sb.AppendLine($"{kv.Key.name}:   {kv.Value.Result}");
            }
            return sb.ToString();
        }
    }
}

