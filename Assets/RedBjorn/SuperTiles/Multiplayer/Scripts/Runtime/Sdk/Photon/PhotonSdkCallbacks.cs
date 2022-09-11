#if PHOTON_UNITY_NETWORKING
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using RedBjorn.Utils;
using System.Collections.Generic;
using System.Linq;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace RedBjorn.SuperTiles.Multiplayer.Sdk
{
    /// <summary>
    /// Class which implements Photon Unity Networking callbacks
    /// </summary>
    public partial class PhotonSdk : global::Photon.Realtime.IOnEventCallback,
                                     global::Photon.Realtime.IConnectionCallbacks,
                                     global::Photon.Realtime.ILobbyCallbacks,
                                     global::Photon.Realtime.IMatchmakingCallbacks,
                                     global::Photon.Realtime.IInRoomCallbacks
    {

        void PropsVerifyOnMasterSwitched(Player newMasterClient)
        {
            if (newMasterClient != PhotonNetwork.LocalPlayer)
            {
                return;
            }
            var players = PhotonNetwork.PlayerList;
            var props = PhotonNetwork.CurrentRoom.CustomProperties;
            string key = null;
            object playerIdObj = null;
            var slots = Controller.CurrentRoom.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                key = SlotGetOwnerKey(i);
                playerIdObj = props.TryGetOrDefault(key);
                if (playerIdObj != null)
                {
                    var playerId = (int)playerIdObj;
                    if (players.Any(p => p.ActorNumber == playerId))
                    {
                        continue;
                    }
                    props[key] = NetworkController.InvalidId;
                }
            }

            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }

        void PropsVerifyOnPlayerLeave(Player playerLeft)
        {
            if (!IsMaster() || !IsInRoom)
            {
                return;
            }
            var props = PhotonNetwork.CurrentRoom.CustomProperties;
            string key = null;
            var slots = Controller.CurrentRoom.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                key = SlotGetOwnerKey(i);
                var val = props.TryGetOrDefault(key);
                if (val != null && (int)val == playerLeft.ActorNumber)
                {
                    props[key] = NetworkController.InvalidId;
                }
            }

            key = PlayerReadyKey(playerLeft.ActorNumber);
            if (props.ContainsKey(key))
            {
                props[key] = false;
            }

            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }

        void PropsUpdate(Hashtable props)
        {
            foreach (var de in props)
            {
                if (de.Key is string key)
                {
                    var slotIndex = SlotGetType(key);
                    if (slotIndex >= 0)
                    {
                        if (slotIndex >= Controller.CurrentRoom.Slots.Count)
                        {
                            Log.E($"Skip updating slot type. Invalid slot index: {slotIndex}");
                        }
                        else
                        {
                            Controller.CurrentRoom.Slots[slotIndex].Type = SquadControllerData.Type((int)de.Value);
                        }
                        continue;
                    }

                    slotIndex = SlotGetOwner(key);
                    if (slotIndex >= 0)
                    {
                        if (slotIndex >= Controller.CurrentRoom.Slots.Count)
                        {
                            Log.E($"Skip updating slot owner. Invalid slot index: {slotIndex}");
                        }
                        else
                        {
                            Controller.CurrentRoom.Slots[slotIndex].Player = Controller.CurrentRoom.Players.FirstOrDefault(p => p.Id == (int)de.Value);
                        }
                        continue;
                    }

                    var playerId = PlayerReadyGetPlayer(key);
                    if (playerId >= 0)
                    {
                        var player = Controller.CurrentRoom.Players.FirstOrDefault(p => p.Id == playerId);
                        if (player != null)
                        {
                            var isReady = (bool)de.Value;
                            var inReadyList = Controller.CurrentRoom.Ready.Contains(player);
                            if (isReady && !inReadyList)
                            {
                                Controller.CurrentRoom.Ready.Add(player);
                            }
                            if (!isReady && inReadyList)
                            {
                                Controller.CurrentRoom.Ready.Remove(player);
                            }
                        }
                        continue;
                    }

                    playerId = SessionLoadedGetPlayer(key);
                    if (playerId >= 0)
                    {
                        var player = Controller.CurrentRoom.Players.FirstOrDefault(p => p.Id == playerId);
                        if (player != null)
                        {
                            var isLoaded = (bool)de.Value;
                            var inLoadedList = Controller.CurrentRoom.Loaded.Contains(player);
                            if (isLoaded && !inLoadedList)
                            {
                                Controller.CurrentRoom.Loaded.Add(player);
                            }
                            if (!isLoaded && inLoadedList)
                            {
                                Controller.CurrentRoom.Loaded.Remove(player);
                            }
                        }
                    }
                }
            }
            if (IsMaster())
            {
                var playerCountCurrent = Controller.CurrentRoom.Slots.Count(s => s.Type != SquadControllerType.Player || s.Player != null);
                var playerCountSaved = props.TryGetOrDefault(PLAYERS_COUNT_KEY);
                if (playerCountSaved == null || playerCountCurrent != (int)playerCountSaved)
                {
                    PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { PLAYERS_COUNT_KEY, playerCountCurrent } });
                }
            }
        }

        void RoomListUpdate(List<RoomInfo> roomList)
        {
            if (roomList != null)
            {
                foreach (var room in roomList)
                {
                    var existed = Controller.LobbyRooms.FirstOrDefault(r => r.Id == room.Name);
                    if (existed != null)
                    {
                        if (room.RemovedFromList || !room.IsOpen || !room.IsVisible)
                        {
                            Controller.LobbyRooms.Remove(existed);
                        }
                        else
                        {
                            var props = room.CustomProperties;
                            var level = props.TryGetOrDefault(LEVEL_KEY);
                            if (level != null)
                            {
                                existed.Level = (int)level;
                            }
                            var turnDuration = props.TryGetOrDefault(TURN_DURATION_KEY);
                            if (turnDuration != null)
                            {
                                existed.TurnDuration = (float)turnDuration;
                            }
                            var players = props.TryGetOrDefault(PLAYERS_COUNT_KEY);
                            if (players != null)
                            {
                                existed.PlayersCount = (int)players;
                            }
                        }
                    }
                    else
                    {
                        if (!room.RemovedFromList && room.IsOpen && room.IsVisible)
                        {
                            var props = room.CustomProperties;
                            var newRoom = new RoomLobbyEntity();
                            newRoom.Id = room.Name;
                            var level = props.TryGetOrDefault(LEVEL_KEY);
                            if (level != null)
                            {
                                newRoom.Level = (int)level;
                            }
                            var turnDuration = props.TryGetOrDefault(TURN_DURATION_KEY);
                            if (turnDuration != null)
                            {
                                newRoom.TurnDuration = (float)turnDuration;
                            }
                            var players = props.TryGetOrDefault(PLAYERS_COUNT_KEY);
                            if (players != null)
                            {
                                newRoom.PlayersCount = (int)players;
                            }
                            Controller.LobbyRooms.Add(newRoom);
                        }
                    }
                }
            }

            foreach (var target in Controller.LobbyRoomTargets)
            {
                target.OnRoomListUpdate();
            }
        }

        void LeaveLobby()
        {
            Log.I("Lobby left");
            Controller.LobbyRooms.Clear();
            RoomListUpdate(null);
        }

        void GetVacantSlot()
        {
            int slot = -1;
            var props = PhotonNetwork.CurrentRoom.CustomProperties;
            foreach (var kv in props)
            {
                if (kv.Key is string key)
                {
                    var slotIndex = SlotGetType(key);
                    if (slotIndex >= 0)
                    {
                        if (SquadControllerData.Type((int)kv.Value) != SquadControllerType.Player)
                        {
                            continue;
                        }
                        var slotOwnerObj = props.TryGetOrDefault(SlotGetOwnerKey(slotIndex));
                        if (slotOwnerObj != null && (int)slotOwnerObj == NetworkController.InvalidId)
                        {
                            slot = slotIndex;
                            break;
                        }
                    }
                }
            }
            if (slot > -1)
            {
                SendRoomSlotChangeOwner(slot, Controller.Player.Id);
            }
            else
            {
                Log.I("No vacant slot");
            }
        }

        void OnBattleAction(EventData photonEvent)
        {
            foreach (var target in Controller.BattleActionTargets)
            {
                target.OnBattleAction(photonEvent.CustomData as byte[]);
            }
        }

        void OnTurnFinishLaunch(EventData photonEvent)
        {
            var turn = (int)photonEvent.CustomData;
            Log.I($"Received {nameof(TURN_FINISH_LAUNCH)} event. Start finishing turn: {turn}");
            foreach (var target in Controller.TurnTargets)
            {
                target.OnTurnFinishLaunched(turn);
            }
        }

        void OnTurnFinishCompleted(EventData photonEvent)
        {
            var player = Controller.CurrentRoom.Players.FirstOrDefault(p => p.Id == photonEvent.Sender);
            var turn = (int)photonEvent.CustomData;
            foreach (var target in Controller.TurnTargets)
            {
                target.OnTurnFinishCompleted(player, turn);
            }
        }

        void OnTurnStart(EventData photonEvent)
        {
            Log.I($"Received {nameof(TURN_START)} event. Begin starting turn");
            foreach (var target in Controller.TurnTargets)
            {
                target.OnTurnStart((double)photonEvent.CustomData);
            }
        }

        void OnError(EventData photonEvent)
        {
            Log.E(photonEvent.CustomData as string);
            var player = PhotonNetwork.PlayerList.First(p => photonEvent.Sender == p.ActorNumber);
            if (player != null)
            {
                PhotonNetwork.RaiseEvent(LEAVE, null, new RaiseEventOptions() { TargetActors = new int[] { player.ActorNumber } }, SendOptions.SendReliable);
            }
            else
            {
                PhotonNetwork.RaiseEvent(LEAVE, null, new RaiseEventOptions() { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
            }
        }

        void OnLeave(EventData photonEvent)
        {
            Log.E("Will leave room because error can't be fixed");
            PhotonNetwork.LeaveRoom();
        }

        #region IOnEventCallback
        void global::Photon.Realtime.IOnEventCallback.OnEvent(EventData photonEvent)
        {
            if (photonEvent.Code == GAME_START)
            {
                GameStartDo();
            }
            else if (photonEvent.Code == SLOT_CHANGE_OWNER)
            {
                SlotChangeOwnerDo(photonEvent);
            }
            else if (photonEvent.Code == SLOT_CHANGE_TYPE)
            {
                SlotChangeTypeDo(photonEvent);
            }
            else if (photonEvent.Code == ACTION)
            {
                OnBattleAction(photonEvent);
            }
            else if (photonEvent.Code == TURN_FINISH_LAUNCH)
            {
                OnTurnFinishLaunch(photonEvent);
            }
            else if (photonEvent.Code == TURN_FINISH_COMPLETED)
            {
                OnTurnFinishCompleted(photonEvent);
            }
            else if (photonEvent.Code == TURN_START)
            {
                OnTurnStart(photonEvent);
            }
            else if (photonEvent.Code == ERROR)
            {
                OnError(photonEvent);
            }
            else if (photonEvent.Code == LEAVE)
            {
                OnLeave(photonEvent);
            }
        }
        #endregion // IOnEventCallback

        #region IConnectionCallbacks
        void global::Photon.Realtime.IConnectionCallbacks.OnConnected()
        {
            Log.I("Low-level connection established");
            Controller.Player = new Photon.Player(PhotonNetwork.LocalPlayer);
        }

        void global::Photon.Realtime.IConnectionCallbacks.OnConnectedToMaster()
        {
            Log.I("Connected to Master Server");
            ConnectedToMaster = true;
            Controller.ConnectionTargets.ForEach(t => t.OnConnected());
        }

        void global::Photon.Realtime.IConnectionCallbacks.OnDisconnected(DisconnectCause cause)
        {
            Log.I($"Disconnected: {cause}");
            ConnectedToMaster = false;
            Controller.Player = null;
            Controller.ConnectionTargets.ToList().ForEach(t => t.OnDisconnected());
        }

        void global::Photon.Realtime.IConnectionCallbacks.OnCustomAuthenticationResponse(Dictionary<string, object> data)
        {

        }

        void global::Photon.Realtime.IConnectionCallbacks.OnCustomAuthenticationFailed(string debugMessage)
        {

        }

        public void OnRegionListReceived(RegionHandler regionHandler)
        {

        }
        #endregion //IConnectionCallbacks

        #region ILobbyCallbacks
        void global::Photon.Realtime.ILobbyCallbacks.OnJoinedLobby()
        {
            Log.I("Lobby joined");
        }

        void global::Photon.Realtime.ILobbyCallbacks.OnLeftLobby()
        {
            LeaveLobby();
        }

        void global::Photon.Realtime.ILobbyCallbacks.OnRoomListUpdate(List<RoomInfo> roomList)
        {
            RoomListUpdate(roomList);
        }

        void global::Photon.Realtime.ILobbyCallbacks.OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
        {

        }
        #endregion //ILobbyCallbacks

        #region IMatchmakingCallbacks
        void global::Photon.Realtime.IMatchmakingCallbacks.OnCreatedRoom()
        {
            Log.I("Room created");
            Controller.RoomTargets.ForEach(r => r.OnCreated());
        }

        void global::Photon.Realtime.IMatchmakingCallbacks.OnCreateRoomFailed(short returnCode, string message)
        {
            Log.I($"Room create failed. Code {returnCode }. Cause {message}");
            Controller.RoomTargets.ForEach(r => r.OnCreateFailed());
        }

        void global::Photon.Realtime.IMatchmakingCallbacks.OnJoinedRoom()
        {
            LeaveLobby();
            Log.I("Room joined");
            var index = 0;
            var props = PhotonNetwork.CurrentRoom.CustomProperties;
            var val = props.TryGetOrDefault(LEVEL_KEY);
            if (val != null)
            {
                index = (int)val;
            }
            var levels = S.Levels.Data;
            if (index < 0 || index >= levels.Count)
            {
                Log.E($"Invalid level index {index}");
                return;
            }
            float turnDuration = Network.TurnDurations[0];
            var turnDurationObj = props.TryGetOrDefault(TURN_DURATION_KEY);
            if (turnDurationObj != null)
            {
                turnDuration = (float)turnDurationObj;
            }
            Controller.CurrentRoom = new RoomEntity(PhotonNetwork.CurrentRoom.Name, index, levels[index], turnDuration);
            foreach (var kv in PhotonNetwork.CurrentRoom.Players)
            {
                if (kv.Value.IsLocal)
                {
                    Controller.CurrentRoom.PlayerAdd(Controller.Player);
                }
                else
                {
                    Controller.CurrentRoom.PlayerAdd(new Photon.Player(kv.Value));
                }
            }
            PropsUpdate(props);
            Controller.RoomTargets.ForEach(r => r.OnJoined());
            GetVacantSlot();
        }

        void global::Photon.Realtime.IMatchmakingCallbacks.OnJoinRoomFailed(short returnCode, string message)
        {
            Log.E($"Room join failed. Code {returnCode }. Cause {message}");
            Controller.RoomTargets.ForEach(r => r.OnJoinedFailed());
        }

        void global::Photon.Realtime.IMatchmakingCallbacks.OnLeftRoom()
        {
            Log.I("Room left");
            Controller.CurrentRoom = null;
            Controller.RoomTargets.ForEach(r => r.OnLeft());
        }

        void global::Photon.Realtime.IMatchmakingCallbacks.OnJoinRandomFailed(short returnCode, string message)
        {
            Log.I($"Room random join failed. Code {returnCode }. Cause {message}");
            Controller.RoomTargets.ForEach(r => r.OnJoinedFailed());
        }

        void global::Photon.Realtime.IMatchmakingCallbacks.OnFriendListUpdate(List<FriendInfo> friendList)
        {

        }
        #endregion //IMatchmakingCallbacks

        #region IInRoomCallbacks
        void global::Photon.Realtime.IInRoomCallbacks.OnPlayerEnteredRoom(Player newPlayer)
        {
            Log.I($"Player id:{newPlayer.ActorNumber} entered room");
            Controller.CurrentRoom.PlayerAdd(new Photon.Player(newPlayer));
            Controller.InRoomTargets.ForEach(r => r.OnPlayerEnteredRoom());
        }

        void global::Photon.Realtime.IInRoomCallbacks.OnPlayerLeftRoom(Player otherPlayer)
        {
            Log.I($"Player id:{otherPlayer.ActorNumber} left room");
            var player = Controller.CurrentRoom.Players.FirstOrDefault(p => p.Id == otherPlayer.ActorNumber);
            Controller.CurrentRoom.PlayerRemove(player);
            PropsVerifyOnPlayerLeave(otherPlayer);
            Controller.InRoomTargets.ForEach(r => r.OnPlayerLeftRoom(player));
        }

        void global::Photon.Realtime.IInRoomCallbacks.OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            Log.I("Room properties updated");
            PropsUpdate(propertiesThatChanged);
            Controller.InRoomTargets.ForEach(r => r.OnRoomPropertiesUpdate());
        }

        void global::Photon.Realtime.IInRoomCallbacks.OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            Log.I("Player properties updated");
            Controller.InRoomTargets.ForEach(r => r.OnPlayerPropertiesUpdate());
        }

        void global::Photon.Realtime.IInRoomCallbacks.OnMasterClientSwitched(Player newMasterClient)
        {
            Log.I($"Master cliend switched to {newMasterClient.ActorNumber}");
            PropsVerifyOnMasterSwitched(newMasterClient);
            Controller.InRoomTargets.ForEach(r => r.OnMasterClientSwitched());
        }
        #endregion //IInRoomCallbacks
    }
}
#endif