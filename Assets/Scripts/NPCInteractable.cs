using UnityEngine;

/// <summary>
/// Attach this to the NPC cylinder.
/// Shows a "Press E to talk" prompt when the player is nearby and opens the chat UI on key press.
/// </summary>
public class NPCInteractable : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionRange = 3f;
    public KeyCode interactionKey = KeyCode.E;
    public Transform player;

    [Header("UI References")]
    [Tooltip("World-space object with the 'Press E to talk' text, placed above the NPC.")]
    public GameObject promptText;
    public ChatUIController chatUIController;

    private bool playerInRange = false;

    void Start()
    {
        if (promptText != null)
        {
            promptText.SetActive(false);
        }

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    void Update()
    {
        if (player == null || chatUIController == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        playerInRange = distance <= interactionRange;

        if (promptText != null)
        {
            promptText.SetActive(playerInRange && !chatUIController.IsChatOpen);
        }

        if (playerInRange && !chatUIController.IsChatOpen && Input.GetKeyDown(interactionKey))
        {
            chatUIController.OpenChat();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
