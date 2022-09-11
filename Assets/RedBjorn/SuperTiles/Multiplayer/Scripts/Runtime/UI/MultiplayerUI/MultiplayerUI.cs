using RedBjorn.SuperTiles.Multiplayer.UI.Multiplayer;
using RedBjorn.SuperTiles.Multiplayer.UI.Multiplayer.States;
using System.Collections.Generic;
using UnityEngine;

namespace RedBjorn.SuperTiles.Multiplayer.UI
{
    /// <summary>
    /// UI panel which provides UI multiplayer elements
    /// </summary>
    public class MultiplayerUI : MonoBehaviour, IRoomCallbacks, IConnectionCallbacks, ILobbyRoomCallbacks
    {
        public LoginUI Login;
        public ConnectedUI Connected;
        public GameObject InProccess;
        public DebugUI Debug;
        public MenuUI Menu;

        MultiplayerUIState CurrentState;

        public bool IsConnected { get; set; }
        public bool IsQuitting { get; private set; }
        public List<LevelData> Levels => S.Levels.Data;

        void OnEnable()
        {
            NetworkController.AddCallbackTarget(this);
            ChangeState(new Login());
        }

        void Update()
        {
            Debug.Show();
        }

        void OnDisable()
        {
            NetworkController.RemoveCallbackTarget(this);
        }

        void OnApplicationQuit()
        {
            IsQuitting = true;
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void ChangeState(MultiplayerUIState state)
        {
            if (CurrentState != null)
            {
                CurrentState.OnExit();
            }
            CurrentState = state;
            if (CurrentState != null)
            {
                CurrentState.Enter(this);
            }
        }

        void IRoomCallbacks.OnJoined()
        {
            if (CurrentState != null)
            {
                CurrentState.OnRoomJoined();
            }
        }

        void IRoomCallbacks.OnJoinedFailed()
        {
            if (CurrentState != null)
            {
                CurrentState.OnRoomJoinFailed();
            }
        }

        void IRoomCallbacks.OnLeft()
        {
            if (CurrentState != null)
            {
                CurrentState.OnRoomLeft();
            }
        }

        void IRoomCallbacks.OnCreated()
        {
            if (CurrentState != null)
            {
                CurrentState.OnRoomCreated();
            }
        }

        void IRoomCallbacks.OnCreateFailed()
        {
            if (CurrentState != null)
            {
                CurrentState.OnRoomCreateFailed();
            }
        }

        void IConnectionCallbacks.OnConnected()
        {
            if (CurrentState != null)
            {
                CurrentState.OnConnected();
            }
        }

        void IConnectionCallbacks.OnDisconnected()
        {
            if (CurrentState != null)
            {
                CurrentState.OnDisconnected();
            }
        }

        void ILobbyRoomCallbacks.OnRoomListUpdate()
        {
            if (CurrentState != null)
            {
                CurrentState.OnRoomListUpdate();
            }
        }
    }
}