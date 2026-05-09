using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class AzureSpeechClient
{
    private readonly string apiKey;
    private readonly string region;
    private readonly string voiceName;

    public AzureSpeechClient(string apiKey, string region, string voiceName)
    {
        this.apiKey = apiKey;
        this.region = region;
        this.voiceName = voiceName;
    }

    // ============ SPEECH-TO-TEXT ============
    public async Task<string> TranscribeAsync(byte[] wavBytes, string language = "de-DE")
    {
        string url = $"https://{region}.stt.speech.microsoft.com/speech/recognition/" +
                     $"conversation/cognitiveservices/v1?language={language}&format=detailed";

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(wavBytes);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Ocp-Apim-Subscription-Key", apiKey);
        req.SetRequestHeader("Content-Type", "audio/wav; codecs=audio/pcm; samplerate=16000");
        req.SetRequestHeader("Accept", "application/json");
        req.timeout = 30;

        var op = req.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[Azure STT] Fehler: {req.error}\n{req.downloadHandler.text}");
            return null;
        }

        var resp = JsonUtility.FromJson<SttResponse>(req.downloadHandler.text);
        if (resp == null || resp.RecognitionStatus != "Success")
        {
            Debug.LogWarning($"[Azure STT] Erkennung fehlgeschlagen: {req.downloadHandler.text}");
            return null;
        }

        Debug.Log($"[Azure STT] Transkript: {resp.DisplayText}");
        return resp.DisplayText;
    }

    [System.Serializable]
    private class SttResponse
    {
        public string RecognitionStatus;
        public string DisplayText;
    }

    // ============ TEXT-TO-SPEECH ============
    public async Task<AudioClip> SynthesizeAsync(string text)
    {
        string url = $"https://{region}.tts.speech.microsoft.com/cognitiveservices/v1";

        // SSML-XML für Azure TTS aufbauen (deutsche Sprache + gewählte Stimme)
        string ssml = $@"<speak version='1.0' xml:lang='de-DE'>
            <voice xml:lang='de-DE' name='{voiceName}'>
                {System.Security.SecurityElement.Escape(text)}
            </voice>
        </speak>";

        byte[] body = System.Text.Encoding.UTF8.GetBytes(ssml);

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(body);
        // Wir kriegen MP3 zurück, das Unity direkt als AudioClip dekodieren kann
        req.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.MPEG);
        req.SetRequestHeader("Ocp-Apim-Subscription-Key", apiKey);
        req.SetRequestHeader("Content-Type", "application/ssml+xml");
        req.SetRequestHeader("X-Microsoft-OutputFormat", "audio-24khz-48kbitrate-mono-mp3");
        req.SetRequestHeader("User-Agent", "BeratungszimmerVR");
        req.timeout = 30;

        var op = req.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[Azure TTS] Fehler: {req.error}\n{req.downloadHandler.text}");
            return null;
        }

        AudioClip clip = DownloadHandlerAudioClip.GetContent(req);
        Debug.Log($"[Azure TTS] AudioClip erhalten: {clip.length:F2} Sekunden");
        return clip;
    }
}