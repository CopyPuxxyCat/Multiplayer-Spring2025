using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ChatUIManager : MonoBehaviour
{
    public static ChatUIManager Instance;
    public GameObject chatPanel;
    public TMP_InputField chatInputField;
    public Transform messageContent;
    public GameObject messagePrefab;

    private bool isChatPanelOpen = false;
    private bool isTyping = false;
    private Coroutine autoCloseCoroutine;

    private void Awake()
    {
        Instance = this;
        chatPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!isChatPanelOpen)
            {
                OpenChatPanel();
                OpenInputField();
            }
            else
            {
                if (!isTyping)
                    OpenInputField();
                else
                    TrySendChat();
            }
        }
    }

    void OpenChatPanel()
    {
        chatPanel.SetActive(true);
        isChatPanelOpen = true;
    }

    void OpenInputField()
    {
        chatInputField.gameObject.SetActive(true);
        chatInputField.text = "";
        chatInputField.ActivateInputField();
        GameInputManager.Instance.LockInput();
        isTyping = true;
    }

    void CloseInputField()
    {
        chatInputField.gameObject.SetActive(false);
        GameInputManager.Instance.UnlockInput();
    }    

    void TrySendChat()
    {
        if (!string.IsNullOrEmpty(chatInputField.text))
        {
            ChatManager.Instance.SendChatMessage(chatInputField.text);
            chatInputField.text = "";
        }
        CloseInputField();
        chatInputField.DeactivateInputField();
        isTyping = false;
    }

    public void SpawnMessage(string sender, string message)
    {
        GameObject newMsg = Instantiate(messagePrefab, messageContent);
        newMsg.GetComponent<TMP_Text>().text = $"<b>{sender}:</b> {message}";

        // Auto scroll xuống dòng mới nhất
        ScrollRect scrollRect = messageContent.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }

        // Reset timer auto close chat
        if (autoCloseCoroutine != null)
            StopCoroutine(autoCloseCoroutine);
        autoCloseCoroutine = StartCoroutine(AutoCloseChatAfterDelay());
    }

    IEnumerator AutoCloseChatAfterDelay()
    {
        yield return new WaitForSeconds(30f);
        CloseChatPanel();
    }

    void CloseChatPanel()
    {
        chatPanel.SetActive(false);
        isChatPanelOpen = false;
        isTyping = false;
        GameInputManager.Instance.UnlockInput();
    }
}


