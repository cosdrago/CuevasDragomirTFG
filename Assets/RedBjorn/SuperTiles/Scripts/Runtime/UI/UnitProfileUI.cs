using RedBjorn.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RedBjorn.SuperTiles.UI
{
    /// <summary>
    /// Class which represents unit inside UI
    /// </summary>
    public class UnitProfileUI : MonoBehaviour
    {
        public Image Avatar;
        public Image EffectTemplate;
        public Transform EffectParent;
        public GameObject Panel;

        List<Image> Effects = new List<Image>();

        void Awake()
        {
            Panel.SetActive(false);
            EffectTemplate.gameObject.SetActive(false);
        }

        public void Show(UnitEntity unit)
        {
            foreach (var s in Effects)
            {
                Spawner.Despawn(s.gameObject);
            }
            Effects.Clear();

            Panel.SetActive(true);
            if (unit != null)
            {
                for (int i = 0; i < unit.Effects.Count; i++)
                {
                    var effectUI = Spawner.Spawn(EffectTemplate, EffectParent);
                    effectUI.sprite = unit.Effects[i].Data.Icon;
                    effectUI.gameObject.SetActive(true);
                    Effects.Add(effectUI);
                }
                Avatar.sprite = unit.Data.Avatar;
            }
            else
            {
                foreach (var icon in Effects)
                {
                    icon.enabled = false;
                }
                Avatar.enabled = false;
            }
        }
    }
}