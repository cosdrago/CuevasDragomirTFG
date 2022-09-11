using RedBjorn.Utils;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RedBjorn.SuperTiles.Multiplayer.UI
{
    /// <summary>
    /// UI panel which provides UI room elements
    /// </summary>
    public class RoomUI : MonoBehaviour, IInRoomCallbacks
    {
        public TextMeshProUGUI LevelText;
        public TextMeshProUGUI TurnDurationText;
        public SquadSlotUI SquadSlotRef;
        public GameObject SpectatorRef;
        public Button StartButton;
        public Button ReadyButton;
        public Button LeaveButton;

        MultiplayerUI Controller;
        LevelData Level;
        List<SquadSlotUI> PlayerSlots = new List<SquadSlotUI>();
        List<GameObject> SpectatorSlots = new List<GameObject>();

        void Awake()
        {
            SquadSlotRef.Hide();
            SpectatorRef.SetActive(false);
        }

        void OnEnable()
        {
            NetworkController.AddCallbackTarget(this);
        }

        void OnDisable()
        {
            NetworkController.RemoveCallbackTarget(this);
        }

        public void Show(RoomEntity room, MultiplayerUI controller)
        {
            Level = room.Level;
            LevelText.text = Level.Caption;
            TurnDurationText.text = $"{room.TurnDuration.ToString("F0")} sec";
            Controller = controller;

            StartButton.onClick.RemoveAllListeners();
            StartButton.onClick.AddListener(() => NetworkController.SendGameStart());

            ReadyButton.onClick.RemoveAllListeners();
            ReadyButton.onClick.AddListener(() => NetworkController.SendIsReady());

            LeaveButton.onClick.RemoveAllListeners();
            LeaveButton.onClick.AddListener(() => NetworkController.RoomLeave());

            UpdateStatus();

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void OnPlayerEnteredRoom()
        {
            UpdateStatus();
        }

        public void OnPlayerLeftRoom(INetworkPlayer player)
        {
            UpdateStatus();
        }

        public void OnRoomPropertiesUpdate()
        {
            UpdateStatus();
        }

        public void OnPlayerPropertiesUpdate()
        {
            UpdateStatus();
        }

        public void OnMasterClientSwitched()
        {
            UpdateStatus();
        }

        void UpdateStatus()
        {
            Controller.Connected.Profile.Refresh();
            var roomData = NetworkController.GetCurrentRoom();
            var isMaster = NetworkController.IsMaster();
            foreach (var p in PlayerSlots)
            {
                Spawner.Despawn(p.gameObject);
            }
            PlayerSlots.Clear();
            var canStart = isMaster;
            var players = new HashSet<INetworkPlayer>();
            var slots = roomData.Slots;
            for (var i = 0; i < slots.Count; i++)
            {
                var playerName = string.Empty;
                var isReady = false;
                if (slots[i].Type == SquadControllerType.AI)
                {
                    isReady = true;
                    playerName = "AI";
                }
                else
                {
                    isReady = slots[i].IsReady();
                    var player = slots[i].Player;
                    if (player != null)
                    {
                        playerName = player.Nickname;
                        players.Add(player);
                    }
                    canStart &= isReady;
                }

                var ui = Spawner.Spawn(SquadSlotRef, SquadSlotRef.transform.parent);
                var slot = i;
                ui.Show(slots[i].Team.name,
                        slots[i].Type,
                        playerName,
                        isReady,
                        isMaster,
                        () => NetworkController.SendRoomSlotChangeOwner(slot, NetworkController.Id),
                        (type) => NetworkController.SendRoomSlotChangeType(slot, type));
                PlayerSlots.Add(ui);
            }

            foreach (var p in SpectatorSlots)
            {
                Spawner.Despawn(p);
            }
            SpectatorSlots.Clear();
            var roomPlayers = roomData.Players;
            for (var i = 0; i < roomPlayers.Count; i++)
            {
                if (players.Contains(roomPlayers[i]))
                {
                    continue;
                }
                var ui = Spawner.Spawn(SpectatorRef, SpectatorRef.transform.parent);
                ui.SetActive(true);
                ui.GetComponentsInChildren<TextMeshProUGUI>()[0].text = roomPlayers[i].Nickname;
                SpectatorSlots.Add(ui);
            }

            ReadyButton.gameObject.SetActive(players.Contains(NetworkController.LocalPlayer));

            StartButton.gameObject.SetActive(isMaster);
            if (StartButton.gameObject.activeSelf)
            {
                StartButton.interactable = canStart;
            }
        }
    }
}