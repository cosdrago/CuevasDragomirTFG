using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RedBjorn.SuperTiles.Multiplayer.UI
{
    /// <summary>
    /// UI panel which provides UI lobby elements
    /// </summary>
    public class LobbyUI : MonoBehaviour
    {
        [Header("MENU")]
        public Button RoomsMenuButton;
        public GameObject RoomsMenuPanel;
        public Button CreateMenuButton;
        public GameObject CreateMenuPanel;
        [Header("JOIN")]
        public Button RoomJoinRef;
        public Button RoomJoin;
        public List<Button> RoomJoins;
        [Header("CREATE")]
        public TMP_InputField RoomName;
        public Button Create;
        public ToggleGroup LevelGroup;
        public Toggle LevelRef;
        public List<Toggle> Levels;
        public TMP_Dropdown TurnDuration;

        void Awake()
        {
            LevelRef.gameObject.SetActive(false);
            CreateMenuButton.onClick.RemoveAllListeners();
            CreateMenuButton.onClick.AddListener(() => SelectMenu(CreateMenuPanel));

            RoomJoinRef.gameObject.SetActive(false);
            RoomsMenuButton.onClick.RemoveAllListeners();
            RoomsMenuButton.onClick.AddListener(() => SelectMenu(RoomsMenuPanel));
        }

        void OnEnable()
        {
            SelectMenu(RoomsMenuPanel);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        void SelectMenu(GameObject selected)
        {
            CreateMenuPanel.SetActive(CreateMenuPanel == selected);
            RoomsMenuPanel.SetActive(RoomsMenuPanel == selected);
        }
    }
}
