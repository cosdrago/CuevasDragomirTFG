using RedBjorn.ProtoTiles;
using RedBjorn.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RedBjorn.SuperTiles
{
    /// <summary>
    /// Unit state
    /// </summary>
    [Serializable]
    public class UnitEntity : UI.ITooltip
    {
        [Serializable]
        public class UnitStatDictionary : SerializableDictionary<UnitStatTag, StatEntity> { } // Hack to serialize dictionary

        public int Id;
        public bool IsDead;
        public UnitData Data;
        public UnitHealthEntity Health;
        public UnitMoveEntity Mover;
        [NonSerialized]
        public UnitView View;
        public UnitStatDictionary Stats;
        [SerializeReference]
        public List<ItemEntity> Items = new List<ItemEntity>();
        [SerializeReference]
        public List<EffectEntity> Effects = new List<EffectEntity>();

        public StatEntity this[UnitStatTag stat] { get { return Stats.TryGetOrDefault(stat); } }
        public MapEntity Map { get { return Game.Battle.Map; } }

        [SerializeField]
        Vector3Int CachedPosition;
        public Vector3Int TilePosition
        {
            get
            {
                if (View != null)
                {
                    return View.Position;
                }
                return CachedPosition;
            }
            set
            {
                CachedPosition = value;
            }
        }

        [SerializeField]
        Quaternion CachedRotation;
        public Quaternion Rotation
        {
            get
            {
                if (View != null)
                {
                    return View.Rotation;
                }
                return CachedRotation;
            }
            set
            {
                CachedRotation = value;
            }
        }

        [NonSerialized]
        GameEntity CachedGame;
        public GameEntity Game
        {
            get
            {
                return CachedGame;
            }
        }

        public Vector3 WorldPosition
        {
            get
            {
                if (View != null)
                {
                    return View.WorldPosition;
                }
                return Vector3.zero;
            }
        }

        public event Action<EffectEntity> OnEffectAdded;
        public event Action<EffectEntity> OnEffectRemoved;

        public UnitEntity(int id, Vector3 position, Quaternion rotation, UnitData data, GameEntity game)
        {
            Id = id;
            Data = data;
            CachedGame = game;
            Stats = new UnitStatDictionary();
            foreach (var s in Data.Stats)
            {
                Stats[s.Stat] = new StatEntity(s.Value);
            }
            Health = new UnitHealthEntity(this);
            Mover = new UnitMoveEntity(this);
            var tile = game.Battle.Map.Tile(position);
            if (tile != null)
            {
                CachedPosition = tile.Position;
            }
            CachedRotation = rotation;
            Data.DefaultItems.ForEach(item => AddItem(item));
            CreateView(position, rotation, game);
            Health.CreateView();
            Mover.CreateView();
        }

        public override string ToString()
        {
            if (Data == null)
            {
                return string.Format("Unit-{0}", Id.ToString());
            }
            return string.Format("{0}-{1}", Data.name, Id.ToString());
        }

        public void Load(GameEntity game)
        {
            CachedGame = game;
            var position = game.Battle.Map.WorldPosition(CachedPosition);
            CreateView(position, CachedRotation, game);
            Health.Load(this);
            Health.CreateView();
            Mover.Load(this);
            Mover.CreateView();
            foreach (var item in Items)
            {
                item.Load();
            }
            foreach (var effect in Effects)
            {
                effect.Load(this);
            }
        }

        public void ViewShow()
        {
            if (View)
            {
                View.Show();
            }
        }

        public void ViewHide()
        {
            if (View)
            {
                View.Hide();
            }
        }

        public void SetPosition(Vector3 point)
        {
            if (!IsDead)
            {
                var tile = Map.Tile(point);
                if (tile != null)
                {
                    TilePosition = tile.Position;
                }
                if (View != null)
                {
                    View.SetPosition(point);
                }
            }
        }

        public IEnumerator LookingAt(Vector3 position)
        {
            var rotating = true;
            Mover.Rotate(position, () => rotating = false);
            while (rotating)
            {
                yield return null;
            }
        }

        public void AddItem(ItemData item)
        {
            var itemEntity = new ItemEntity(item);
            Items.Add(itemEntity);
        }

        public IEnumerator AddEffect(EffectData effect, int duration, BattleEntity battle)
        {
            if (effect)
            {
                EffectEntity entity = null;
                if (battle.Level.EqualEffectInfluenceDuration)
                {
                    entity = Effects.FirstOrDefault(e => e.Data == effect);
                    if (entity != null)
                    {
                        entity.DurationAdd(duration);
                    }
                }
                if (entity == null)
                {
                    entity = new EffectEntity(effect, duration, this);
                    Effects.Add(entity);
                    yield return entity.OnAdded(battle);
                }
                OnEffectAdded.SafeInvoke(entity);
            }
        }

        public IEnumerator RemoveEffect(EffectData effect, BattleEntity battle)
        {
            if (effect)
            {
                var effectsRemove = Effects.Where(e => e.Data == effect).ToList();
                foreach (var ef in effectsRemove)
                {
                    yield return RemoveEffect(ef, battle);
                }
            }
        }

        public IEnumerator RemoveEffect(EffectEntity effect, BattleEntity battle)
        {
            if (effect != null)
            {
                Effects.Remove(effect);
                yield return effect.OnRemoved(battle);
                OnEffectRemoved.SafeInvoke(effect);
            }
        }

        public IEnumerator OnStartTurn(BattleEntity battle)
        {
            foreach (var effect in Effects)
            {
                yield return effect.Handle(battle);
            }

            var effectsRemove = Effects.Where(effect => effect.Duration <= 0).ToList();
            foreach (var effect in effectsRemove)
            {
                yield return RemoveEffect(effect, battle);
            }
        }

        public IEnumerator OnFinishTurn(BattleEntity battle)
        {
            Items.ForEach(item => item.OnFinishTurn());
            yield break;
        }

        void CreateView(Vector3 position, Quaternion rotation, GameEntity game)
        {
            View = Spawner.Spawn(S.Prefabs.UnitView);
            View.transform.position = position;
            View.transform.rotation = rotation;
            View.Init(game, this);
        }

        public string Text()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"{ToString()} \n");
            sb.AppendLine($"Health:   {Health.Text()}");
            var health = S.Battle.Tags.Unit.Health;
            foreach (var k in Stats.Where(kv => kv.Key != health))
            {
                sb.AppendLine($"{k.Key.name}:   {k.Value.Result}");
            }
            sb.AppendLine($"Effects: ");
            if (Effects.Count == 0)
            {
                sb.AppendLine("None ");
            }
            else
            {
                foreach (var effect in Effects)
                {
                    sb.AppendLine($"{effect} ");
                }
            }
            return sb.ToString();
        }

        public void StatUpdate(UnitStatTag unitStat, StatEntity stat)
        {
            Stats[unitStat] = stat;
            Health.Update();
        }

        public static void Heal(UnitEntity target, float power, UnitEntity healer, ItemEntity item, BattleEntity battle)
        {
            Change(target, power, healer, item, battle);
        }

        public static void Damage(UnitEntity target, float power, BattleEntity battle)
        {
            Change(target, -power, null, null, battle);
        }

        public static void Damage(UnitEntity target, float power, UnitEntity damager, ItemEntity item, BattleEntity battle)
        {
            Change(target, -power, damager, item, battle);
        }

        static void Change(UnitEntity target, float delta, UnitEntity changer, ItemEntity item, BattleEntity battle)
        {
            if (target == null || target.Health == null)
            {
                Log.W("Can't change health to unknown target");
                return;
            }
            var convertedDelta = delta;
            if (battle.Level.Health)
            {
                convertedDelta = battle.Level.Health.Handle(delta, target, changer, item);
            }
            target.Health.Change(convertedDelta);
            if (target.IsDead)
            {
                battle.Map.UnRegisterUnit(target);
            }
        }
    }
}
