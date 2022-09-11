using RedBjorn.SuperTiles.UI;
using RedBjorn.SuperTiles.Utils;
using RedBjorn.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedBjorn.SuperTiles
{
    /// <summary>
    /// View which represents UnitHealthEntity at unity scene by operating Sprite component
    /// </summary>
    public class UnitHealthView : MonoBehaviour
    {
        public UnitStatTag MaxHealthTag;
        public BarSprite HealthBarPrefab;
        public EffectUnitWorldUI EffectPrefab;
        public float FillSpeed;

        BarSprite Bar;
        UnitEntity Unit;
        Coroutine HealthProcessingCoroutine;
        Queue<float> Values = new Queue<float>();

        float MaxHealth { get { return Unit[MaxHealthTag]; } }
        Settings.BattleSettings.HealthSettings Health => S.Battle.Health;

        void OnDestroy()
        {
            if (Unit != null)
            {
                Unit.Health.OnHeathChanged -= OnHealthChanged;
                Unit.View.OnStateChanged -= OnViewStateChanged;
            }
        }

        public void Init(float current, float previous, UnitStatTag maxHealthTag, UnitEntity unit)
        {
            MaxHealthTag = maxHealthTag;
            FillSpeed = Health.BarSpeedFill;
            var prefabs = S.Prefabs;
            var transformTags = S.Battle.Tags.Transform;
            HealthBarPrefab = prefabs.HealthBar;
            Unit = unit;
            Unit.Health.OnHeathChanged += OnHealthChanged;
            Unit.View.OnStateChanged += OnViewStateChanged;
            Bar = Spawner.Spawn(HealthBarPrefab, Vector3.zero, Quaternion.identity);
            Bar.transform.SetParent(Unit.View.GetTransformHolder(transformTags.UiHolder));
            Bar.transform.localPosition = Vector3.zero;
            Bar.gameObject.SetActive(true);
            OnHealthChanged(new HealthChangeContext { Current = current, MaxHealth = MaxHealth, Previous = previous });
        }

        void OnViewStateChanged(bool state)
        {
            Bar.gameObject.SetActive(state);
        }

        void OnHealthChanged(HealthChangeContext context)
        {
            var val = Mathf.Clamp01(context.Current / MaxHealth);
            Values.Enqueue(val);
            HandleHealthChange();
        }

        void HandleHealthChange()
        {
            if (HealthProcessingCoroutine == null && Values.Count > 0)
            {
                if (gameObject.activeInHierarchy)
                {
                    var val = Values.Dequeue();
                    HealthProcessingCoroutine = StartCoroutine(HealthProcessing(val));
                }
            }
        }

        IEnumerator HealthProcessing(float newVal)
        {
            var original = Bar.WidthCurrent;
            var target = newVal * Bar.WidthMax;
            var speed = Mathf.Abs(original - target) * FillSpeed;
            if (Mathf.Abs(speed) > 0.001f)
            {
                var time = 0f;
                while (time < 1f)
                {
                    Bar.WidthCurrent = Mathf.Lerp(original, target, time);
                    yield return null;
                    time += Time.deltaTime * speed;
                }
            }
            Bar.WidthCurrent = target;
            if (Bar.WidthCurrent <= 0f)
            {
                if (Health.BarOnDeathHide)
                {
                    Bar.gameObject.SetActive(false);
                }
                else
                {
                    Bar.Background.color = Health.BarOnDeathColor;
                }
            }
            HealthProcessingCoroutine = null;
            HandleHealthChange();
        }
    }
}
