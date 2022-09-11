using RedBjorn.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RedBjorn.SuperTiles.UI
{
    public class TeamPanelUI : MonoBehaviour
    {
        public Transform TeamSelectorParent;
        public UnitSelectorUI UnitSelector;
        public UnitProfileUI UnitProfile;

        List<UnitSelectorUI> Selectors = new List<UnitSelectorUI>();

        void Awake()
        {
            UnitSelector.gameObject.SetActive(false);
        }

        public void Init(List<UnitEntity> squad, UnitEntity current, Action<UnitEntity> onSelectUnit)
        {
            foreach (var s in Selectors)
            {
                Spawner.Despawn(s.gameObject);
            }
            Selectors.Clear();

            for (int i = 0; i < squad.Count; i++)
            {
                var s = Spawner.Spawn(UnitSelector, TeamSelectorParent);
                s.Init(onSelectUnit, squad[i], i + 1, squad[i] == current);
                Selectors.Add(s);
            }

            UnitProfile.Show(current);
        }
    }
}