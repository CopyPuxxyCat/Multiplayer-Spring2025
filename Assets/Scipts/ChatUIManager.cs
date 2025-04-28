using UnityEngine;
using TMPro; // nhớ dùng TextMeshPro
using UnityEngine.UI;

public class ChatUIManager : MonoBehaviour
{
    public static ChatUIManager Instance;
    public GameObject chatPanel;
    public TMP_InputField chatInputField;
    public Transform messageContent;
    public GameObject messagePrefab;

    private bool isChatting = false;

    private void Awake()
    {
        Instance = this;
        chatPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!isChatting)
                OpenChat();
            else
                SendChat();
        }
    }

    void OpenChat()
    {
        chatPanel.SetActive(true);
        chatInputField.text = "";
        chatInputField.ActivateInputField();
        isChatting = true;
        GameInputManager.Instance.LockInput(); // khóa bắn, khóa move
    }

    void SendChat()
    {
        if (!string.IsNullOrEmpty(chatInputField.text))
        {
            ChatManager.Instance.SendChatMessage(chatInputField.text);
        }
        chatPanel.SetActive(false);
        isChatting = false;
        GameInputManager.Instance.UnlockInput(); // mở lại input
    }

    public void SpawnMessage(string sender, string message)
    {
        GameObject newMsg = Instantiate(messagePrefab, messageContent);
        newMsg.GetComponent<TMP_Text>().text = $"<b>{sender}:</b> {message}";
    }
}

