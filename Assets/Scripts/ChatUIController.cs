using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the on-screen chat panel: input field, send button, response text.
/// Talks to HuggingFaceChatClient to get the NPC's reply.
/// </summary>
public class ChatUIController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject chatPanel;
    public InputField inputField;
    public Text responseText;
    public Button sendButton;
    public Button closeButton;

    [Header("References")]
    public HuggingFaceChatClient chatClient;
    public PlayerController playerController;

    public bool IsChatOpen { get; private set; }

    void Start()
    {
        if (chatPanel != null)
        {
            chatPanel.SetActive(false);
        }

        if (sendButton != null)
        {
            sendButton.onClick.AddListener(SendMessageToNPC);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseChat);
        }
    }

    void Update()
    {
        if (!IsChatOpen) return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SendMessageToNPC();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseChat();
        }
    }

    public void OpenChat()
    {
        IsChatOpen = true;

        if (chatPanel != null) chatPanel.SetActive(true);
        if (playerController != null) playerController.SetMovementEnabled(false);

        if (inputField != null)
        {
            inputField.text = "";
            inputField.ActivateInputField();
        }
    }

    public void CloseChat()
    {
        IsChatOpen = false;

        if (chatPanel != null) chatPanel.SetActive(false);
        if (playerController != null) playerController.SetMovementEnabled(true);
    }

    void SendMessageToNPC()
    {
        if (inputField == null || string.IsNullOrWhiteSpace(inputField.text)) return;
        if (chatClient == null) return;

        string userMessage = inputField.text;
        inputField.text = "";

        if (responseText != null)
        {
            responseText.text = "NPC is thinking...";
        }

        chatClient.SendMessageToAI(userMessage, OnResponseReceived, OnErrorReceived);
    }

    void OnResponseReceived(string reply)
    {
        if (responseText != null)
        {
            responseText.text = "NPC: " + reply;
        }
    }

    void OnErrorReceived(string error)
    {
        if (responseText != null)
        {
            responseText.text = "Error: " + error;
        }
    }
}
