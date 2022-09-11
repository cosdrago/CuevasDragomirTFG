using RedBjorn.Utils;
using System;
using UnityEngine;

namespace RedBjorn.SuperTiles
{
    /// <summary>
    /// Health state of unit
    /// </summary>
    [Serializable]
    public class UnitHealthEntity
    {
        [SerializeField]
        float Current;
        [SerializeField]
        float Ratio;

        [NonSerialized]
        UnitEntity Unit;
        UnitHealthView View;
        UnitStatTag MaxTag;
        StatEntity Max => Unit[MaxTag];

        public event Action<HealthChangeContext> OnHeathChanged;

        public UnitHealthEntity(UnitEntity unit)
        {
            Unit = unit;
            MaxTag = S.Battle.Tags.Unit.Health;
            Current = Max;
            Ratio = 1f;
        }

        public void Load(UnitEntity unit)
        {
            Unit = unit;
            MaxTag = S.Battle.Tags.Unit.Health;
        }

        public void CreateView()
        {
            View = Unit.View.gameObject.AddComponent<UnitHealthView>();
            View.Init(Current, Current, MaxTag, Unit);
        }

        public void Update()
        {
            if (Unit.IsDead)
            {
                return;
            }
            if (Current <= 0f)
            {
                Unit.IsDead = true;
                return;
            }

            var previous = Current;
            if (Current > Max)
            {
                Current = Max * Ratio;
            }
            Ratio = Current / Max;
            if (Mathf.Abs(Current - previous) < 0.01f)
            {
                OnHeathChanged.SafeInvoke(new HealthChangeContext() { Current = Current, Previous = previous, MaxHealth = Max });
            }
        }

        public void Change(float delta)
        {
            if (Unit.IsDead)
            {
                return;
            }

            var previous = Current;
            Current += delta;
            if (Current <= 0f)
            {
                Unit.IsDead = true;
            }
            if (Current > Max)
            {
                Current = Max;
            }
            Ratio = Current / Max;
            OnHeathChanged.SafeInvoke(new HealthChangeContext() { Current = Current, Previous = previous, MaxHealth = Max });
        }

        public string Text()
        {
            return $"{Current}/{Max.Result}";
        }
    }
}