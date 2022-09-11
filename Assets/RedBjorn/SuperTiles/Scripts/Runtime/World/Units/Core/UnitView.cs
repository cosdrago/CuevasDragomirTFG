using RedBjorn.ProtoTiles;
using RedBjorn.Utils;
using System;
using System.Linq;
using UnityEngine;

namespace RedBjorn.SuperTiles
{
    /// <summary>
    /// View which represents UnitEntity at unity scene
    /// </summary>
    public class UnitView : MonoBehaviour
    {
        GameEntity Game;
        UnitEntity Unit;
        GameObject Model;

        public InteractableGameobject Interactable { get; private set; }
        public TransformTagHolder[] TransformHolders { get; private set; }
        MapEntity Map { get { return Game.Battle.Map; } }

        public event Action<bool> OnStateChanged;

        public Vector3Int Position
        {
            get
            {
                if (Map != null)
                {
                    var tile = Map.Tile(transform.position);
                    if (tile != null)
                    {
                        return tile.Position;
                    }
                    return Vector3Int.zero;
                }
                return Vector3Int.zero;
            }
        }

        public Quaternion Rotation
        {
            get
            {
                return transform.rotation;
            }
        }

        public Vector3 WorldPosition
        {
            get
            {
                return transform.position;
            }
        }

        public void Init(GameEntity game, UnitEntity unit)
        {
            Game = game;
            Unit = unit;
            Model = Spawner.Spawn(Unit.Data.Model, transform);
            Model.transform.localPosition = Vector3.zero;
            Model.transform.localRotation = Quaternion.identity;
            name = string.Format("Unit-{0}", Unit);
            Model.name = "Model";
            Interactable = gameObject.AddComponent<InteractableGameobject>();
            Interactable.Init(Game);
            TransformHolders = GetComponentsInChildren<TransformTagHolder>(true);
            var unitInit = GetComponentsInChildren<IUnitInitialize>(true);
            foreach (var c in unitInit)
            {
                c.Init(Unit);
            }
            var ui = GetTransformHolder(S.Battle.Tags.Transform.UiHolder);
            if (ui)
            {
                ui.localPosition = Unit.Data.UiHolder;
                var effectPrefab = S.Prefabs.EffectUI;
                if (effectPrefab)
                {
                    var effect = Spawner.Spawn(effectPrefab, ui);
                    effect.Init(Unit);
                }
            }
        }

        public void Show()
        {
            Model.SetActive(true);
            OnStateChanged.SafeInvoke(true);
        }

        public void Hide()
        {
            Model.SetActive(false);
            OnStateChanged.SafeInvoke(false);
        }

        public void SetPosition(Vector3 point)
        {
            transform.position = point;
        }

        public Transform GetTransformHolder(TransformTag tag)
        {
            var holder = TransformHolders.FirstOrDefault(t => t.Tag == tag);
            return holder ? holder.transform : null;
        }
    }
}
