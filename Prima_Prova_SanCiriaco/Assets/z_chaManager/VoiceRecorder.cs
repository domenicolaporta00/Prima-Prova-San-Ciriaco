using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class VoiceRecorder : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button recordButton;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private ChatManager chatManager;

    [Header("STT Endpoint")]
    [SerializeField] private string sttUrl = "http://127.0.0.1:5000/transcribe";

    private AudioClip recordedClip;
    private string micDevice;
    private bool isRecording = false;

    [Serializable]
    private class STTResponse
    {
        public string text;
    }

    private void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            micDevice = Microphone.devices[0];
            if (recordButton != null)
            {
                recordButton.onClick.AddListener(ToggleRecording);
            }
        }
        else
        {
            Debug.LogError("[VoiceRecorder] Nessun microfono trovato!");
            if (recordButton != null) recordButton.interactable = false;
        }
    }

    private void ToggleRecording()
    {
        if (!isRecording)
        {
            // Registra fino a 10 secondi a 16 kHz (frequenza ottimale per Whisper)
            recordedClip = Microphone.Start(micDevice, false, 10, 16000);
            isRecording = true;
            if (buttonText != null) buttonText.text = "Ferma";
        }
        else
        {
            Microphone.End(micDevice);
            isRecording = false;
            if (buttonText != null) buttonText.text = "Elaboro...";
            recordButton.interactable = false;

            StartCoroutine(SendAudioToWhisperCoroutine());
        }
    }

    private IEnumerator SendAudioToWhisperCoroutine()
    {
        byte[] wavData = SavWav.GetWavBytes(recordedClip);

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", wavData, "audio.wav", "audio/wav");

        using (UnityWebRequest request = UnityWebRequest.Post(sttUrl, form))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                STTResponse res = JsonUtility.FromJson<STTResponse>(request.downloadHandler.text);
                string transcript = res?.text ?? "";

                if (!string.IsNullOrEmpty(transcript))
                {
                    Debug.Log($"[Whisper]: {transcript}");
                    if (chatManager != null)
                    {
                        chatManager.InviaDomandaVocale(transcript);
                    }
                }
            }
            else
            {
                Debug.LogError($"[Whisper Error]: {request.error}\n{request.downloadHandler.text}");
            }
        }

        if (buttonText != null) buttonText.text = "Parla";
        recordButton.interactable = true;
    }
}