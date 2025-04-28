using Photon.Chat;
using Photon.Pun;
using ExitGames.Client.Photon;
using UnityEngine;
using Photon.Chat.Demo;

public class ChatManager : MonoBehaviour, IChatClientListener
{
    public static ChatManager Instance;
    private ChatClient chatClient;
    public string userName;
    private string currentChannel;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        userName = PhotonNetwork.NickName;
        ConnectToChat();
    }

    void Update()
    {
        chatClient?.Service(); // rất quan trọng!
    }

    void ConnectToChat()
    {
        chatClient = new ChatClient(this);
        ChatAppSettings chatSettings = PhotonNetwork.PhotonServerSettings.AppSettings.GetChatSettings();
        chatClient.Connect(chatSettings.AppIdChat, "1.0", new AuthenticationValues(userName));
    }

    public void SendChatMessage(string message)
    {
        if (!string.IsNullOrEmpty(message))
            chatClient.PublishMessage(currentChannel, message);
    }

    public void OnConnected()
    {
        Debug.Log("Connected to Photon Chat!");
        currentChannel = PhotonNetwork.CurrentRoom.Name;
        chatClient.Subscribe(new string[] { currentChannel });
    }

    public void OnDisconnected() { }
    public void OnChatStateChange(ChatState state) { }
    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        for (int i = 0; i < senders.Length; i++)
        {
            ChatUIManager.Instance.SpawnMessage(senders[i], messages[i].ToString());
        }
    }
    public void OnPrivateMessage(string sender, object message, string channelName) { }
    public void OnSubscribed(string[] channels, bool[] results) { }
    public void OnUnsubscribed(string[] channels) { }
    public void OnStatusUpdate(string user, int status, bool gotMessage, object message) { }
    public void DebugReturn(DebugLevel level, string message) { }

    public void OnUserSubscribed(string channel, string user)
    {
        throw new System.NotImplementedException();
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        throw new System.NotImplementedException();
    }
}

