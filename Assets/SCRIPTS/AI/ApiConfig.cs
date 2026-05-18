using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ApiConfig", menuName = "AI/API Config")]
public class ApiConfig : ScriptableObject
{
    [Header("API Keys - NICHT in Git committen!")]
    public string anthropicApiKey;
    public string azureSpeechKey;

    [Header("Azure Speech Settings")]
    [Tooltip("z.B. germanywestcentral oder westeurope")]
    public string azureRegion = "germanywestcentral";

    [Tooltip("Default-Stimme. Im UI ueberschreibbar.")]
    public string voiceName = "de-DE-FlorianMultilingualNeural";

    [Header("Verfuegbare deutsche Stimmen (fuer UI-Dropdown)")]
    public List<VoiceOption> availableVoices = new List<VoiceOption>
    {
        new VoiceOption("Florian (m)",   "de-DE-FlorianMultilingualNeural"),
        new VoiceOption("Seraphina (w)", "de-DE-SeraphinaMultilingualNeural"),
        new VoiceOption("Katja (w)",     "de-DE-KatjaNeural"),
        new VoiceOption("Conrad (m)",    "de-DE-ConradNeural"),
    };

    [Header("Claude Settings")]
    public string claudeModel = "claude-sonnet-4-6";
    public int maxTokens = 300;

    [Header("System-Prompt (Default - im UI ueberschreibbar)")]
    [TextArea(8, 20)]
    public string systemPrompt =
        "Du bist eine ratsuchende Person in einem virtuellen " +
        "Beratungszimmer. Ein Berater oder eine Beraterin spricht " +
        "gerade mit dir. Antworte ausschliesslich auf Deutsch, in " +
        "kurzen, natuerlichen Saetzen (1-3 Saetze).\n\n" +
        "Du hast ein konkretes Anliegen, ueber das du sprechen moechtest. " +
        "Reagiere emotional und authentisch wie eine echte Person - " +
        "manchmal unsicher, manchmal ausweichend, manchmal nachdenklich. " +
        "Sei nicht zu kooperativ - du brauchst Zeit, dich zu oeffnen.";
}

[System.Serializable]
public class VoiceOption
{
    public string displayName;  // Was im Dropdown angezeigt wird
    public string azureVoiceId; // Was an Azure geschickt wird

    public VoiceOption(string display, string azureId)
    {
        displayName = display;
        azureVoiceId = azureId;
    }
}