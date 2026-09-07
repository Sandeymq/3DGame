using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Sends the player's message to the free Hugging Face Inference API (conversational task)
/// and returns the NPC's generated reply.
///
/// Setup:
/// 1. Create a free account at https://huggingface.co
/// 2. Create an access token at https://huggingface.co/settings/tokens (read access is enough)
/// 3. Paste the token into the "Api Token" field in the Inspector
/// 4. Optionally change "Model Name" to any other model that supports the "conversational" task
///    (for example "microsoft/DialoGPT-medium")
/// </summary>
public class HuggingFaceChatClient : MonoBehaviour
{
    [Header("Hugging Face API Settings")]
    [Tooltip("Get a free token at https://huggingface.co/settings/tokens")]
    public string apiToken = "PASTE_YOUR_HUGGING_FACE_TOKEN_HERE";

    [Tooltip("Free conversational model. Can be replaced with any other model that supports the conversational task.")]
    public string modelName = "facebook/blenderbot-400M-distill";

    private string ApiUrl => "https://api-inference.huggingface.co/models/" + modelName;

    private readonly List<string> pastUserInputs = new List<string>();
    private readonly List<string> generatedResponses = new List<string>();

    /// <summary>
    /// Sends a user message to the AI service and invokes onSuccess with the reply,
    /// or onError with an error message.
    /// </summary>
    public void SendMessageToAI(string userMessage, Action<string> onSuccess, Action<string> onError)
    {
        StartCoroutine(SendRequestCoroutine(userMessage, onSuccess, onError));
    }

    private IEnumerator SendRequestCoroutine(string userMessage, Action<string> onSuccess, Action<string> onError)
    {
        HFConversationInput conversationInput = new HFConversationInput
        {
            text = userMessage,
            past_user_inputs = pastUserInputs.ToArray(),
            generated_responses = generatedResponses.ToArray()
        };

        HFRequestBody requestBody = new HFRequestBody
        {
            inputs = conversationInput
        };

        string jsonBody = JsonUtility.ToJson(requestBody);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(ApiUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            if (!string.IsNullOrEmpty(apiToken) && apiToken != "PASTE_YOUR_HUGGING_FACE_TOKEN_HERE")
            {
                request.SetRequestHeader("Authorization", "Bearer " + apiToken);
            }

            yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            bool failed = request.result != UnityWebRequest.Result.Success;
#else
            bool failed = request.isNetworkError || request.isHttpError;
#endif
            if (failed)
            {
                onError?.Invoke(request.error + " | " + request.downloadHandler.text);
                yield break;
            }

            string responseJson = request.downloadHandler.text;

            if (responseJson.Contains("\"error\""))
            {
                HFErrorResponse errorResponse = JsonUtility.FromJson<HFErrorResponse>(responseJson);
                if (errorResponse != null && errorResponse.estimated_time > 0f)
                {
                    onError?.Invoke("The model is still loading, please try again in about " +
                        Mathf.RoundToInt(errorResponse.estimated_time) + " seconds.");
                }
                else
                {
                    onError?.Invoke("API error: " + responseJson);
                }
                yield break;
            }

            HFConversationResponse conversationResponse = JsonUtility.FromJson<HFConversationResponse>(responseJson);

            if (conversationResponse != null && !string.IsNullOrEmpty(conversationResponse.generated_text))
            {
                pastUserInputs.Add(userMessage);
                generatedResponses.Add(conversationResponse.generated_text);
                onSuccess?.Invoke(conversationResponse.generated_text);
            }
            else
            {
                onError?.Invoke("Could not parse the response from the AI service.");
            }
        }
    }

    /// <summary>
    /// Clears the conversation history (call this if you want the NPC to "forget" the previous chat).
    /// </summary>
    public void ResetConversation()
    {
        pastUserInputs.Clear();
        generatedResponses.Clear();
    }
}

[Serializable]
public class HFConversationInput
{
    public string text;
    public string[] past_user_inputs;
    public string[] generated_responses;
}

[Serializable]
public class HFRequestBody
{
    public HFConversationInput inputs;
}

[Serializable]
public class HFConversationState
{
    public string[] past_user_inputs;
    public string[] generated_responses;
}

[Serializable]
public class HFConversationResponse
{
    public string generated_text;
    public HFConversationState conversation;
}

[Serializable]
public class HFErrorResponse
{
    public string error;
    public float estimated_time;
}
