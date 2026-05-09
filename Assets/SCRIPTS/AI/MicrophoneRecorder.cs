using UnityEngine;
using System;
using System.IO;

public class MicrophoneRecorder : MonoBehaviour
{
    [SerializeField] private int sampleRate = 16000;
    [SerializeField] private int maxRecordingSeconds = 30;

    private AudioClip recordingClip;
    private string deviceName;
    private bool isRecording;

    public bool IsRecording => isRecording;
    public event Action<byte[]> OnRecordingFinished;

    void Start()
    {
        // Android-Permission zur Laufzeit anfordern (wichtig auf Quest)
        #if PLATFORM_ANDROID
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(
                UnityEngine.Android.Permission.Microphone))
        {
            UnityEngine.Android.Permission.RequestUserPermission(
                UnityEngine.Android.Permission.Microphone);
        }
        #endif

        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("[MicRecorder] Kein Mikrofon gefunden!");
            return;
        }
        deviceName = Microphone.devices[0];
        Debug.Log($"[MicRecorder] Mikrofon: {deviceName}");
    }

    public void ToggleRecording()
    {
        if (isRecording) StopRecording();
        else StartRecording();
    }

    private void StartRecording()
    {
        Debug.Log("[MicRecorder] Aufnahme gestartet");
        recordingClip = Microphone.Start(
            deviceName, false, maxRecordingSeconds, sampleRate);
        isRecording = true;
    }

    private void StopRecording()
    {
        if (!isRecording) return;
        int recordedSamples = Microphone.GetPosition(deviceName);
        Microphone.End(deviceName);
        isRecording = false;
        Debug.Log($"[MicRecorder] gestoppt: {recordedSamples} samples");

        if (recordedSamples == 0)
        {
            Debug.LogWarning("[MicRecorder] Leere Aufnahme verworfen");
            return;
        }

        float[] samples = new float[recordedSamples * recordingClip.channels];
        recordingClip.GetData(samples, 0);

        byte[] wavBytes = EncodeWav(samples, recordingClip.channels, sampleRate);
        OnRecordingFinished?.Invoke(wavBytes);
    }

    private byte[] EncodeWav(float[] samples, int channels, int sr)
    {
        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);
        // RIFF
        w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
        w.Write(36 + samples.Length * 2);
        w.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
        // fmt
        w.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
        w.Write(16);
        w.Write((short)1);
        w.Write((short)channels);
        w.Write(sr);
        w.Write(sr * channels * 2);
        w.Write((short)(channels * 2));
        w.Write((short)16);
        // data
        w.Write(System.Text.Encoding.ASCII.GetBytes("data"));
        w.Write(samples.Length * 2);
        foreach (float s in samples)
            w.Write((short)(Mathf.Clamp(s, -1f, 1f) * short.MaxValue));
        return ms.ToArray();
    }
}