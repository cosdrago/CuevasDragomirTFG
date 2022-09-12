using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;
using Photon.Chat;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour, IChatClientListener
{
    private ChatClient chatClient;

    public TextMeshProUGUI channelName;

    public TMP_InputField plrName;
    public TextMeshProUGUI connectionState;
    public TMP_InputField msgInput;
    public TextMeshProUGUI msgArea;

    public TMP_InputField[] channels;

    public TMP_InputField trymy;

    public GameObject intoPanel;
    public GameObject msgPanel;

    private string worldchat;
    [SerializeField] private string userID;

    // Start is called before the first frame update
    void Start()
    {
        Application.runInBackground = true;
        if (string.IsNullOrEmpty(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat))
        {
            Debug.LogError("No AppID Provided");
            return;
        }



        worldchat = "world";
    }

    // Update is called once per frame
    void Update()
    {
        if (chatClient != null)
        {
            chatClient.Service();
        }



    }

    public void GetConnected()
    {
        Debug.Log("Connecting");
        chatClient = new ChatClient(this);
        chatClient.Connect(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat, PhotonNetwork.AppVersion,
            new Photon.Chat.AuthenticationValues(plrName.text));
    }

    public void GetDisconnected()
    {
        Debug.Log("Leaving");
        chatClient.Disconnect(ChatDisconnectCause.None);
    }

    public void SendMsg()
    {

        chatClient.PublishMessage(worldchat, msgInput.text);

    }


    public void OnLanza()
    {
        // if (chatMessage.text.Trim() == "")
        //    return;
        string texto = "Ha sacado un " + UnityEngine.Random.Range(0, 10);
        // get our player
       chatClient.PublishMessage(worldchat, texto);

        //chatMessage.text = "";
    }
    public void DebugReturn(DebugLevel level, string message)
    {

    }

    public void OnDisconnected()
    {
        intoPanel.SetActive(true);
        msgPanel.SetActive(false);
        chatClient.Unsubscribe(new string[] { worldchat });
        chatClient.SetOnlineStatus(ChatUserStatus.Offline);
    }

    public void OnConnected()
    {
        intoPanel.SetActive(false);
        msgPanel.SetActive(true);
        chatClient.Subscribe(new string[] { worldchat });
        chatClient.SetOnlineStatus(ChatUserStatus.Online);

    }

    public void OnChatStateChange(ChatState state)
    {

    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        for (int i = 0; i < senders.Length; i++)
        {
            msgArea.text += senders[i] + ": " + messages[i] + ", ";
        }
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {

    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        foreach (var channel in channels)
        {
            this.chatClient.PublishMessage(channel, "joined");
        }

        connectionState.text = "Connected";
    }

    public void OnUnsubscribed(string[] channels)
    {
        msgArea.text = "";
    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {

    }

    public void OnUserSubscribed(string channel, string user)
    {

    }

    public void OnUserUnsubscribed(string channel, string user)
    {

    }
}