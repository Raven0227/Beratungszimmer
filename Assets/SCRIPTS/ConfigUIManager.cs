using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConfigUIManager : MonoBehaviour
{
    // PlayerPrefs-Keys (Konstanten - keine Tippfehler-Quelle)
    public const string PREF_VOICE = "ConfigVoice";
    public const string PREF_PROMPT = "ConfigPrompt";

    [Header("UI-Referenzen")]
    [SerializeField] private TMP_Dropdown voiceDropdown;
    [SerializeField] private TMP_InputField promptInputField;
    [SerializeField] private Button applyButton;
    [SerializeField] private Button resetDefaultsButton;

    [Header("Config-Asset (Defaults)")]
    [SerializeField] private ApiConfig config;

    void Start()
    {
        if (config == null)
        {
            Debug.LogError("[ConfigUI] ApiConfig nicht zugewiesen!");
            return;
        }

        SetupDropdown();
        LoadCurrentValues();
        SetupButtons();
    }

    private void SetupDropdown()
    {
        voiceDropdown.ClearOptions();
        foreach (var voice in config.availableVoices)
        {
            voiceDropdown.options.Add(
                new TMP_Dropdown.OptionData(voice.displayName));
        }
        voiceDropdown.RefreshShownValue();
    }

    private void LoadCurrentValues()
    {
        // Voice: gespeicherten Wert holen, sonst Default aus Config
        string savedVoice = PlayerPrefs.GetString(PREF_VOICE, config.voiceName);
        int voiceIndex = config.availableVoices.FindIndex(
            v => v.azureVoiceId == savedVoice);
        if (voiceIndex >= 0)
            voiceDropdown.value = voiceIndex;
        voiceDropdown.RefreshShownValue();

        // Prompt: gespeicherten Wert holen, sonst Default
        string savedPrompt = PlayerPrefs.GetString(PREF_PROMPT, config.systemPrompt);
        promptInputField.text = savedPrompt;
    }

    private void SetupButtons()
    {
        applyButton.onClick.AddListener(ApplyChanges);
        if (resetDefaultsButton != null)
            resetDefaultsButton.onClick.AddListener(ResetToDefaults);
    }

    public void ApplyChanges()
    {
        // Stimme speichern
        string selectedVoiceId = config.availableVoices[voiceDropdown.value].azureVoiceId;
        PlayerPrefs.SetString(PREF_VOICE, selectedVoiceId);

        // Prompt speichern
        PlayerPrefs.SetString(PREF_PROMPT, promptInputField.text);

        PlayerPrefs.Save();
        Debug.Log($"[ConfigUI] Einstellungen gespeichert. Stimme: {selectedVoiceId}");
    }

    public void ResetToDefaults()
    {
        PlayerPrefs.DeleteKey(PREF_VOICE);
        PlayerPrefs.DeleteKey(PREF_PROMPT);
        PlayerPrefs.Save();

        // UI aktualisieren mit Defaults
        LoadCurrentValues();
        Debug.Log("[ConfigUI] Einstellungen auf Defaults zurueckgesetzt");
    }
}