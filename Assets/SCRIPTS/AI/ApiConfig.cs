using UnityEngine;

[CreateAssetMenu(fileName = "ApiConfig", menuName = "AI/API Config")]
public class ApiConfig : ScriptableObject
{
    [Header("API Keys - NICHT in Git committen!")]
    public string anthropicApiKey;
    public string azureSpeechKey;

    [Header("Azure Speech Settings")]
    [Tooltip("z.B. germanywestcentral oder westeurope")]
    public string azureRegion = "germanywestcentral";

    [Tooltip("Deutsche Stimme - z.B. de-DE-FlorianMultilingualNeural (m) oder de-DE-SeraphinaMultilingualNeural (w)")]
    public string voiceName = "de-DE-FlorianMultilingualNeural";

    [Header("Claude Settings")]
    public string claudeModel = "claude-sonnet-4-5";
    public int maxTokens = 300;

    [TextArea(5, 15)]
    public string systemPrompt =
        "Du bist ein professioneller Berater in einem virtuellen " +
        "Beratungszimmer. Antworte ausschliesslich auf Deutsch, " +
        "kurz und natuerlich (1-3 Saetze). Sei empathisch und " +
        "stelle Rueckfragen wenn das Anliegen unklar ist.";
}