using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RedBjorn.SuperTiles.Multiplayer.UI
{
    /// <summary>
    /// UI panel which provides UI squad slot elements
    /// </summary>
    public class SquadSlotUI : MonoBehaviour
    {
        public TextMeshProUGUI SlotName;
        public TextMeshProUGUI OwnerName;
        public TextMeshProUGUI StatusText;
        public Button SelectButon;
        public TMP_Dropdown TypeDropdown;

        Action<int> OnChangeType;

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show(string squadName,
                         SquadControllerType squadTypeOwner,
                         string ownerName,
                         bool isReady,
                         bool canChangeType,
                         Action OnSelect,
                         Action<int> onChangeType)
        {
            OnChangeType = onChangeType;
            TypeDropdown.onValueChanged.RemoveAllListeners();
            TypeDropdown.options = new List<TMP_Dropdown.OptionData>();
            foreach (var type in SquadControllerData.Types())
            {
                TypeDropdown.options.Add(new TMP_Dropdown.OptionData() { text = type });
            }
            TypeDropdown.value = SquadControllerData.TypeIndex(squadTypeOwner);
            TypeDropdown.interactable = canChangeType;

            var name = string.Empty;
            var slotInteractable = true;
            if (!string.IsNullOrEmpty(ownerName))
            {
                name = ownerName;
                slotInteractable = false;
            }

            var statusText = "Not Ready";
            if (isReady)
            {
                statusText = "Ready";
            }

            SlotName.text = squadName;
            OwnerName.text = name;
            StatusText.text = statusText;
            SelectButon.interactable = slotInteractable;
            SelectButon.onClick.RemoveAllListeners();
            SelectButon.onClick.AddListener(() => OnSelect?.Invoke());
            TypeDropdown.onValueChanged.AddListener(OnValueChanged);
            gameObject.SetActive(true);
        }

        void OnValueChanged(int val)
        {
            OnChangeType?.Invoke(val);
        }
    }
}
