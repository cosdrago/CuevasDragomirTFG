using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManagerTFG : NetworkManager
{
    //OnStartClient
    //OnClientConnect
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
    }


    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        Debug.Log($"Hay {numPlayers} jugadores conectados");
    }
}
