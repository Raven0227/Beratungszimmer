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
    [Tooltip("Input Action zum Starten/Stoppen der Aufnahme. Optional - Leertaste funktioniert auch im Editor.")]
    [SerializeField] private InputActionReference toggleAction;

    [Header("Debug")]
    [SerializeField] private bool useSpacebarInEditor = true;

    private AzureSpeechClient azureSpeech;
    private ClaudeClient claude;
    private AudioSource audioSource;
    private bool isProcessing;

    public bool IsSpeaking => audioSource != null && audioSource.isPlaying;
    public AudioSource AudioSource => audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (config == null)
        {
            Debug.LogError("[AIConvo] ApiConfig nicht zugewiesen!");
            return;
        }

        azureSpeech = new AzureSpeechClient(
            config.azureSpeechKey,
            config.azureRegion,
            config.voiceName);

        claude = new ClaudeClient(
            config.anthropicApiKey,
            config.claudeModel,
            config.maxTokens,
            config.systemPrompt);

        if (recorder != null)
            recorder.OnRecordingFinished += HandleAudioReady;
        else
            Debug.LogError("[AIConvo] MicrophoneRecorder nicht zugewiesen!");
    }

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
        // Editor-Komfort: Leertaste zum Testen ohne VR
        if (useSpacebarInEditor && Application.isEditor)
        {
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                ToggleRecording();
            }
        }
    }

    private void OnToggle(InputAction.CallbackContext _)
    {
        ToggleRecording();
    }

    private void ToggleRecording()
    {
        if (isProcessing)
        {
            Debug.Log("[AIConvo] Noch am Verarbeiten, bitte warten...");
            return;
        }
        recorder.ToggleRecording();
    }

    private async void HandleAudioReady(byte[] wavBytes)
    {
        isProcessing = true;
        try
        {
            Debug.Log("[AIConvo] -> Azure STT");
            string transcript = await azureSpeech.TranscribeAsync(wavBytes, "de-DE");
            if (string.IsNullOrWhiteSpace(transcript))
            {
                Debug.LogWarning("[AIConvo] Kein Transkript erhalten");
                return;
            }

            Debug.Log($"[AIConvo] Du sagtest: \"{transcript}\"");

            Debug.Log("[AIConvo] -> Claude");
            string reply = await claude.SendMessageAsync(transcript);
            if (string.IsNullOrWhiteSpace(reply))
            {
                Debug.LogWarning("[AIConvo] Keine Antwort von Claude");
                return;
            }

            Debug.Log("[AIConvo] -> Azure TTS");
            AudioClip clip = await azureSpeech.SynthesizeAsync(reply);
            if (clip == null)
            {
                Debug.LogWarning("[AIConvo] Keine Audio-Antwort erhalten");
                return;
            }

            audioSource.clip = clip;
            audioSource.Play();
            Debug.Log("[AIConvo] -> Antwort wird abgespielt");
        }
        finally
        {
            isProcessing = false;
        }
    }

    public void ResetSession()
    {
        claude?.ResetHistory();
        Debug.Log("[AIConvo] Konversations-History zurückgesetzt");
    }
}