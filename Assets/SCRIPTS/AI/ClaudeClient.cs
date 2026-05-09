using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class ClaudeClient
{
    private readonly string apiKey;
    private readonly string model;
    private readonly int maxTokens;
    private readonly string systemPrompt;
    private readonly List<Message> history = new();

    private const string ENDPOINT = "https://api.anthropic.com/v1/messages";

    public ClaudeClient(string apiKey, string model,
                        int maxTokens, string systemPrompt)
    {
        this.apiKey = apiKey;
        this.model = model;
        this.maxTokens = maxTokens;
        this.systemPrompt = systemPrompt;
    }

    public void ResetHistory() => history.Clear();

    public async Task<string> SendMessageAsync(string userMessage)
    {
        history.Add(new Message { role = "user", content = userMessage });

        var body = new
        {
            model,
            max_tokens = maxTokens,
            system = systemPrompt,
            messages = history
        };
        string json = JsonConvert.SerializeObject(body);

        using var req = new UnityWebRequest(ENDPOINT, "POST");
        req.uploadHandler = new UploadHandlerRaw(
            System.Text.Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("x-api-key", apiKey);
        req.SetRequestHeader("anthropic-version", "2023-06-01");
        req.timeout = 30;

        var op = req.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[Claude] Fehler: {req.error}\n{req.downloadHandler.text}");
            history.RemoveAt(history.Count - 1);
            return null;
        }

        var resp = JsonConvert.DeserializeObject<ClaudeResponse>(
            req.downloadHandler.text);
        string reply = resp.content[0].text;
        history.Add(new Message { role = "assistant", content = reply });
        Debug.Log($"[Claude] Antwort: {reply}");
        return reply;
    }

    private class Message
    {
        public string role;
        public string content;
    }

    private class ClaudeResponse
    {
        public ContentBlock[] content;
    }

    private class ContentBlock
    {
        public string text;
    }
}