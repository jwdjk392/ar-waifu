using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TextCore.Text;

[Serializable]
public class OllamaMessage
{
    public string role;
    public string content;
}

[Serializable]
public class OllamaRequest
{
    public string model;
    public OllamaMessage[] messages;
    public bool stream = false;
}

[Serializable]
public class OllamaResponse
{
    public OllamaChoice[] choices;
}

[Serializable]
public class OllamaChoice
{
    public OllamaMessage message;
}

public class AIManager : MonoBehaviour
{
    [Header("Ollama Settings")]
    public string endpoint =
        "http://localhost:11434/v1/chat/completions";

    public string model = "qwen3.5:9b";

    [Header("Global System Configuration")]
    [TextArea(5, 15)]
    [SerializeField]
    private string systemPrompt =
        "You are a friendly anime character. Respond in Korean.\n\n" +
        "# Animation Control Rules\n" +
        "You can control the character's animations through special animation tags embedded in your responses.\n" +
        "Available Animation Tags\n" +
        "- `[ANIMATION:WAVING]`\n" +
        "- `[ANIMATION:CARCRASH]`\n\n" +
        "RULES:\n"+
        "Animation tags are OPTIONAL.\n"+
        "Do NOT output an animation tag for ordinary dialogue.\n"+
        "Only emit an animation tag when a meaningful animation change is appropriate.\n"+
        "Animation tags may appear anywhere in your response, including the middle or end.\n"+
        "You may emit multiple animation tags in one response when the character's emotional state or actions change.\n"+
        "Only use the predefined animation tags listed above.\n"+
        "Never explain or mention animation tags to the user.\n"+
        "When no animation change is necessary, output natural dialogue without any animation tags.\n\n"+
        "EXAMPLES:\n"+
        "Dialogue:\n"+
        "안녕?\n"+
        "Response:\n"+
        "[ANIMATION:WAVING]\n"+
        "만나서 정말 반가워!";

    [Header("Game Settings")]
    [SerializeField] ARModelManager aRModelManager;
    [SerializeField] UIManager uIManager;
    private List<OllamaMessage> messages =
        new List<OllamaMessage>();

    private bool isSending;
    private bool isReady = false;

    private void Awake()
    {
        ResetConversation();
    }

    public void ResetConversation()
    {
        messages.Clear();
        messages.Add(new OllamaMessage
        {
            role = "system",
            content = systemPrompt
        }
        );
    }

    public void SetupCharacter(CharacterData character)
    {
        messages.Add(new OllamaMessage
        {
            role = "system",
            content = character.systemPrompt
        }
        );
        isReady = true;
    }

    public void Send(string userMessage)
    {
        if (isSending || string.IsNullOrWhiteSpace(userMessage) || !isReady)
            return;

        StartCoroutine(SendCoroutine(userMessage));
    }

    private IEnumerator SendCoroutine(string userMessage)
    {
        aRModelManager.SetAnimation(CharacterMotion.Thinking);
        isSending = true;
        Debug.Log("Processing input... MESSAGE: " + userMessage);

        messages.Add(new OllamaMessage
        {
            role = "user",
            content = userMessage
        });

        var requestData = new OllamaRequest
        {
            model = model,
            messages = messages.ToArray(),
            stream = false
        };

        string json = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request =
            new UnityWebRequest(endpoint, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(
                Encoding.UTF8.GetBytes(json));

            request.downloadHandler =
                new DownloadHandlerBuffer();

            request.SetRequestHeader(
                "Content-Type", "application/json");

            request.timeout = 180;

            yield return request.SendWebRequest();
            aRModelManager.SetAnimation(CharacterMotion.Idle);
            if (request.result ==
                UnityWebRequest.Result.Success)
            {
                var result = JsonUtility.FromJson<OllamaResponse>(
                    request.downloadHandler.text);

                if (result?.choices != null &&
                    result.choices.Length > 0 &&
                    result.choices[0].message != null)
                {
                    string reply =
                        result.choices[0].message.content;

                    messages.Add(new OllamaMessage
                    {
                        role = "assistant",
                        content = reply
                    });

                    Debug.Log("AI: " + reply);
                    OnResponseReceived(reply);
                }
                else
                {
                    Debug.LogError("Invalid API response");
                    messages.RemoveAt(messages.Count - 1);
                }
            }
            else
            {
                Debug.LogError(
                    $"HTTP {request.responseCode}: " +
                    request.error + "\n" +
                    request.downloadHandler.text);
                uIManager.AlertMessageFailure();
                messages.RemoveAt(messages.Count - 1);
            }
        }

        isSending = false;
    }

    private void OnResponseReceived(string response)
    {
        // Connect to your chat UI here.
        Regex AnimationRegex = new Regex(@"\[ANIMATION:(\w+)\]");
        string dialogue = AnimationRegex.Replace(
            response,
            match =>
            {
                string animation = match.Groups[1].Value;
                TryApplyAnimation(animation);
                return "";
            }
        ).Trim();
        uIManager.AddNewMessageToHistory(response);
    }

    private void TryApplyAnimation(string animation)
    {
        switch (animation)
        {
            case "WAVING":
                aRModelManager.SetAnimation(CharacterMotion.Waving);
                return;
            case "CARCRASH":
                aRModelManager.SetAnimation(CharacterMotion.Fainting);
                return;
            default:
                return;
        }
    }
}
