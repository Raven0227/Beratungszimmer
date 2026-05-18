using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class AIConversationManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private ApiConfig config;

    [Header("References")]
    [SerializeField] private MicrophoneRecorder recorder;

    [Header("Input")]
    [Tooltip("z.B. XRI Right Interaction/Activate")]
    [SerializeField] private InputActionReference toggleAction;

    [Header("Debug")]
    [SerializeField] private bool useSpacebarInEditor = true;

    private AzureSpeechClient azureSpeech;
    private ClaudeClient claude;
    private AudioSource audioSource;
    private bool isProcessing;

    // Runtime-Override-Werte (vom UI gesetzt)
    private string currentVoiceName;
    private string currentSystemPrompt;

    public bool IsSpeaking => audioSource != null && audioSource.isPlaying;
    public AudioSource AudioSource => audioSource;
    public ApiConfig Config => config;

   void Awake()
{
    audioSource = GetComponent<AudioSource>();
    if (config == null)
    {
        Debug.LogError("[AIConvo] ApiConfig nicht zugewiesen!");
        return;
    }

    // Werte aus PlayerPrefs laden (falls vorhanden), sonst Defaults
    currentVoiceName = PlayerPrefs.GetString(
        ConfigUIManager.PREF_VOICE, config.voiceName);
    currentSystemPrompt = PlayerPrefs.GetString(
        ConfigUIManager.PREF_PROMPT, config.systemPrompt);

    Debug.Log($"[AIConvo] Geladene Stimme: {currentVoiceName}");

    InitializeClients();

    if (recorder != null)
        recorder.OnRecordingFinished += HandleAudioReady;
}

    private void InitializeClients()
    {
        azureSpeech = new AzureSpeechClient(
            config.azureSpeechKey,
            config.azureRegion,
            currentVoiceName);

        claude = new ClaudeClient(
            config.anthropicApiKey,
            config.claudeModel,
            config.maxTokens,
            currentSystemPrompt);
    }

    // ============ PUBLIC API FUER UI ============

    /// <summary>
    /// Aenderung der Stimme. Setzt die History NICHT zurueck.
    /// </summary>
    public void SetVoice(string azureVoiceId)
    {
        currentVoiceName = azureVoiceId;
        // Nur Azure neu erstellen, Claude-History bleibt
        azureSpeech = new AzureSpeechClient(
            config.azureSpeechKey, config.azureRegion, currentVoiceName);
        Debug.Log($"[AIConvo] Stimme gewechselt: {azureVoiceId}");
    }

    /// <summary>
    /// Aenderung des System-Prompts. Setzt History automatisch zurueck,
    /// da neue Persoenlichkeit = neuer Charakter = keine alte Erinnerung.
    /// </summary>
    public void SetSystemPrompt(string newPrompt)
    {
        currentSystemPrompt = newPrompt;
        claude = new ClaudeClient(
            config.anthropicApiKey,
            config.claudeModel,
            config.maxTokens,
            currentSystemPrompt);
        Debug.Log($"[AIConvo] System-Prompt aktualisiert + History zurueckgesetzt");
    }

    /// <summary>
    /// Manueller Reset der Konversations-History (z.B. fuer neuen Probanden).
    /// </summary>
    public void ResetSession()
    {
        claude?.ResetHistory();
        Debug.Log("[AIConvo] Session zurueckgesetzt");
    }

    // ============ INPUT-HANDLING ============

    void OnEnable()
    {
        if (toggleAction != null && toggleAction.action != null)
        {
            toggleAction.action.Enable();
            toggleAction.action.performed += OnToggle;
        }
    }

    void OnDisable()
    {
        if (toggleAction != null && toggleAction.action != null)
        {
            toggleAction.action.performed -= OnToggle;
            toggleAction.action.Disable();
        }
    }

    void Update()
    {
        if (useSpacebarInEditor && Application.isEditor)
        {
            if (Keyboard.current != null &&
                Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                ToggleRecording();
            }
        }
    }

    private void OnToggle(InputAction.CallbackContext _) => ToggleRecording();

    private void ToggleRecording()
    {
        if (isProcessing) return;
        recorder.ToggleRecording();
    }

    private async void HandleAudioReady(byte[] wavBytes)
    {
        isProcessing = true;

        // Latenz-Messung
        var totalStopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var sttStopwatch = System.Diagnostics.Stopwatch.StartNew();
            string transcript = await azureSpeech.TranscribeAsync(wavBytes, "de-DE");
            sttStopwatch.Stop();
            if (string.IsNullOrWhiteSpace(transcript)) return;
            Debug.Log($"[AIConvo] STT-Latenz: {sttStopwatch.ElapsedMilliseconds} ms");

            var claudeStopwatch = System.Diagnostics.Stopwatch.StartNew();
            string reply = await claude.SendMessageAsync(transcript);
            claudeStopwatch.Stop();
            if (string.IsNullOrWhiteSpace(reply)) return;
            Debug.Log($"[AIConvo] Claude-Latenz: {claudeStopwatch.ElapsedMilliseconds} ms");

            var ttsStopwatch = System.Diagnostics.Stopwatch.StartNew();
            AudioClip clip = await azureSpeech.SynthesizeAsync(reply);
            ttsStopwatch.Stop();
            if (clip == null) return;
            Debug.Log($"[AIConvo] TTS-Latenz: {ttsStopwatch.ElapsedMilliseconds} ms");

            audioSource.clip = clip;
            audioSource.Play();

            totalStopwatch.Stop();
            Debug.Log($"[AIConvo] GESAMT-Latenz: {totalStopwatch.ElapsedMilliseconds} ms");
        }
        finally
        {
            isProcessing = false;
        }
    }
}