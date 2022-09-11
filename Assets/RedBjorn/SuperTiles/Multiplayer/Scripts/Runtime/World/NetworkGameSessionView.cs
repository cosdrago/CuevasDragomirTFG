using RedBjorn.SuperTiles.UI;
using System.Linq;
using UnityEngine;

namespace RedBjorn.SuperTiles.Multiplayer
{
    /// <summary>
    /// Monobehaviour which provides network and Unity callbacks during battle session
    /// </summary>
    public class NetworkGameSessionView : MonoBehaviour, IInRoomCallbacks, IConnectionCallbacks, IRoomCallbacks
    {
        BattleView View;

        void OnEnable()
        {
            NetworkController.AddCallbackTarget(this);
        }

        void OnDisable()
        {
            NetworkController.RemoveCallbackTarget(this);
        }

        void OnDestroy()
        {
            NetworkController.RoomLeave();
            Log.I("NetworkGameSessionView destroyed");
        }

        public void Init(BattleView view)
        {
            View = view;
        }

        void IInRoomCallbacks.OnPlayerEnteredRoom()
        {

        }

        void IInRoomCallbacks.OnPlayerLeftRoom(INetworkPlayer player)
        {
            var entity = View.Battle.Players.FirstOrDefault(p => p.Id == player.Id);
            if (entity != null)
            {
                View.Pause();
                ConfirmMessageUI.Show($"Player {player.Nickname} left",
                                       "Leave",
                                       null,
                                       () =>
                                       {
                                           View.Unpause();
                                           View.DoMenuMain();
                                       },
                                       null);
            }
        }

        void IInRoomCallbacks.OnRoomPropertiesUpdate()
        {

        }

        void IInRoomCallbacks.OnPlayerPropertiesUpdate()
        {

        }

        void IInRoomCallbacks.OnMasterClientSwitched()
        {

        }

        void IConnectionCallbacks.OnConnected()
        {

        }

        void IConnectionCallbacks.OnDisconnected()
        {
            ConfirmMessageUI.Show($"Network issue. Menu will be loaded",
                       "OK",
                       null,
                       () =>
                       {
                           View.Unpause();
                           View.DoMenuMain();
                       },
                       null);
        }

        void IRoomCallbacks.OnJoined()
        {

        }

        void IRoomCallbacks.OnJoinedFailed()
        {

        }

        void IRoomCallbacks.OnLeft()
        {
            ConfirmMessageUI.Show($"Network issue. Menu will be loaded",
                       "OK",
                       null,
                       () =>
                       {
                           View.Unpause();
                           View.DoMenuMain();
                       },
                       null);
        }

        void IRoomCallbacks.OnCreated()
        {

        }

        void IRoomCallbacks.OnCreateFailed()
        {

        }
    }
}