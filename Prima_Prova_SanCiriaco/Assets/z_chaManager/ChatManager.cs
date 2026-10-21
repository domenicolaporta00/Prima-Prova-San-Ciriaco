using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    [Header("Riferimenti UI")]
    [SerializeField] private TMP_InputField inputDomanda;
    [SerializeField] private Button bottoneInvia;
    [SerializeField] private TextMeshProUGUI testoRisposta;

    [Header("Stop audio")]
    [Tooltip("Opzionale: bottone Stop dedicato (compare solo mentre elabora o parla)")]
    [SerializeField] private Button bottoneStop;
    [Tooltip("Opzionale: la X del pannello, ferma anche l'audio quando chiudi")]
    [SerializeField] private Button bottoneChiudi;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSourceVoce;

    [Header("Endpoint Locali")]
    [SerializeField] private string urlOllama = "http://127.0.0.1:11435/api/chat";
    [SerializeField] private string urlTTS = "http://127.0.0.1:5000/tts";
    [SerializeField] private string nomeModello = "gemma3:4b";

    [Header("Parametri LLM")]
    [SerializeField] private float temperatura = 0.3f;
    [Tooltip("Contesto totale (prompt di sistema + domanda + risposta). Alzalo se le risposte sono molto lunghe.")]
    [SerializeField] private int dimensioneContesto = 4096;
    [SerializeField] private string keepAlive = "30m";

    [Header("Lunghezza risposta")]
    [Tooltip("-1 = nessun limite (Ollama si ferma quando il modello ha finito)")]
    [SerializeField] private int maxTokenRisposta = -1;

    [TextArea(5, 16)]
    [SerializeField] private string istruzioniSistema =
        "Sei la guida virtuale del Duomo di San Ciriaco (noto anche semplicemente come Duomo di Ancona o Cattedrale di San Ciriaco) e del suo piazzale sul Colle Guasco.\n" +
        "Considera 'Duomo di Ancona' e 'Duomo di San Ciriaco' come lo stesso identico edificio storico.\n" +
        "Rispondi sempre in italiano, in modo accogliente e chiaro.\n" +
        "Non usare elenchi, asterischi, markdown o emoji: scrivi solo frasi discorsive, adatte a essere lette ad alta voce.\n" +
        "Usa solo le informazioni qui sotto: se non conosci la risposta, dillo onestamente senza inventare.\n\n" +
        "Fatti chiave:\n" +
        "- Posizione: Colle Guasco, vista panoramica sul mare e porto di Ancona.\n" +
        "- Origini: sorge sull'antico tempio greco di Afrodite Euplea.\n" +
        "- Architettura: stile romanico con influssi bizantini, pianta a croce greca.\n" +
        "- Portale: protiro sostenuto dai caratteristici leoni in marmo rosso di Verona.\n" +
        "- Cripta: custodisce le reliquie del patrono San Ciriaco.\n" +
        "- Cupola: a pianta dodecagonale tra le più antiche d'Italia.\n\n" +
        "Se la domanda non riguarda la cattedrale, il piazzale o la sua storia, rispondi cortesemente che sei una guida specializzata solo su San Ciriaco.\n\n" +
        "Rispondi in modo completo ma scorrevole, adatto alla lettura ad alta voce, senza ripetizioni inutili.";

    [Serializable]
    private class Messaggio
    {
        public string role;
        public string content;
    }

    [Serializable]
    private class OpzioniOllama
    {
        public float temperature;
        public int num_ctx;
        public int num_predict;
    }

    [Serializable]
    private class RichiestaChat
    {
        public string model;
        public Messaggio[] messages;
        public bool stream;
        public string keep_alive;
        public OpzioniOllama options;
    }

    [Serializable]
    private class RispostaOllama
    {
        public Messaggio message;
        public bool done;
        public string done_reason;
    }

    [Serializable]
    private class TTSPayload
    {
        public string text;
    }

    // Ogni volta che si preme Stop (o parte una nuova domanda) cambia: le coroutine
    // con un numero vecchio si interrompono da sole.
    private int generazione;
    private bool ttsInCorso;

    private void Start()
    {
        if (bottoneInvia != null) bottoneInvia.onClick.AddListener(InviaDomanda);
        if (bottoneStop != null) bottoneStop.onClick.AddListener(FermaAudio);
        if (bottoneChiudi != null) bottoneChiudi.onClick.AddListener(FermaAudio);

        if (audioSourceVoce == null) audioSourceVoce = GetComponent<AudioSource>();

        if (bottoneStop != null) bottoneStop.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (bottoneStop == null) return;

        bool elaborando = bottoneInvia != null && !bottoneInvia.interactable;
        bool parla = audioSourceVoce != null && audioSourceVoce.isPlaying;
        bool mostra = elaborando || ttsInCorso || parla;

        if (bottoneStop.gameObject.activeSelf != mostra)
            bottoneStop.gameObject.SetActive(mostra);
    }

    // ------------------------------------------------------------------
    // STOP
    // ------------------------------------------------------------------
    public void FermaAudio()
    {
        generazione++;           // annulla richieste Ollama/TTS in corso
        ttsInCorso = false;

        if (audioSourceVoce != null) audioSourceVoce.Stop();
        if (bottoneInvia != null) bottoneInvia.interactable = true;

        if (testoRisposta != null && testoRisposta.text == "Sto pensando...")
            testoRisposta.text = "";
    }

    // ------------------------------------------------------------------
    // Invio domande
    // ------------------------------------------------------------------
    public void InviaDomanda()
    {
        if (inputDomanda == null || testoRisposta == null) return;

        string prompt = inputDomanda.text.Trim();
        if (string.IsNullOrEmpty(prompt)) return;

        FermaAudio(); // se stava ancora parlando, interrompe la risposta precedente

        bottoneInvia.interactable = false;
        testoRisposta.text = "Sto pensando...";

        StartCoroutine(ChiamaOllamaCoroutine(prompt));
    }

    public void InviaDomandaVocale(string domandaTrascritto)
    {
        FermaAudio();

        if (inputDomanda != null) inputDomanda.text = domandaTrascritto;
        if (bottoneInvia != null) bottoneInvia.interactable = false;
        if (testoRisposta != null) testoRisposta.text = "Sto pensando...";

        StartCoroutine(ChiamaOllamaCoroutine(domandaTrascritto));
    }

    // ------------------------------------------------------------------
    // Pulizia del testo (nessun limite di lunghezza):
    // toglie solo markdown e simboli che il TTS leggerebbe male.
    // ------------------------------------------------------------------
    private string PulisciRisposta(string testo)
    {
        if (string.IsNullOrWhiteSpace(testo)) return testo;

        testo = Regex.Replace(testo, @"[*#_`~>]", "");
        testo = Regex.Replace(testo, @"\s+", " ").Trim();
        return testo;
    }

    private IEnumerator ChiamaOllamaCoroutine(string promptUtente)
    {
        int mia = generazione;

        RichiestaChat payload = new RichiestaChat
        {
            model = nomeModello,
            stream = false,
            keep_alive = keepAlive,
            options = new OpzioniOllama
            {
                temperature = temperatura,
                num_ctx = dimensioneContesto,
                num_predict = maxTokenRisposta   // -1 = illimitato
            },
            messages = new Messaggio[]
            {
                new Messaggio { role = "system", content = istruzioniSistema },
                new Messaggio { role = "user", content = promptUtente }
            }
        };

        string jsonPayload = JsonUtility.ToJson(payload);
        byte[] bodyData = Encoding.UTF8.GetBytes(jsonPayload);

        using (UnityWebRequest richiesta = new UnityWebRequest(urlOllama, "POST"))
        {
            richiesta.uploadHandler = new UploadHandlerRaw(bodyData);
            richiesta.downloadHandler = new DownloadHandlerBuffer();
            richiesta.SetRequestHeader("Content-Type", "application/json");

            yield return richiesta.SendWebRequest();

            // Premuto Stop (o nuova domanda) mentre aspettavo: scarta la risposta
            if (mia != generazione) yield break;

            if (richiesta.result == UnityWebRequest.Result.Success)
            {
                RispostaOllama risposta = JsonUtility.FromJson<RispostaOllama>(richiesta.downloadHandler.text);
                string grezzo = risposta?.message?.content ?? "Nessuna risposta ricevuta.";
                string testoGenerato = PulisciRisposta(grezzo);

                Debug.Log($"[Ollama] Risposta ({grezzo.Length} -> {testoGenerato.Length} caratteri, motivo fine: {risposta?.done_reason}): {testoGenerato}");

                testoRisposta.text = testoGenerato;
                StartCoroutine(ChiamaTTSCoroutine(testoGenerato, mia));
            }
            else
            {
                Debug.LogError($"[Ollama Error]: {richiesta.error}\n{richiesta.downloadHandler.text}");
                testoRisposta.text = "Errore di connessione a Ollama.";
            }
        }

        inputDomanda.text = "";
        bottoneInvia.interactable = true;
    }

    private IEnumerator ChiamaTTSCoroutine(string testoDaLeggere, int mia)
    {
        ttsInCorso = true;

        TTSPayload payload = new TTSPayload { text = testoDaLeggere };
        string json = JsonUtility.ToJson(payload);
        byte[] bodyData = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest req = new UnityWebRequest(urlTTS, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(bodyData);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();
            if (mia != generazione) yield break;

            if (req.result == UnityWebRequest.Result.Success)
            {
                string tempPath = System.IO.Path.Combine(Application.persistentDataPath, "temp_tts.wav");
                System.IO.File.WriteAllBytes(tempPath, req.downloadHandler.data);
                Debug.Log($"[TTS] File salvato: {tempPath} ({req.downloadHandler.data.Length} bytes)");

                string uri = "file:///" + tempPath.Replace("\\", "/");
                using (UnityWebRequest audioReq = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.WAV))
                {
                    yield return audioReq.SendWebRequest();
                    if (mia != generazione) yield break;

                    if (audioReq.result == UnityWebRequest.Result.Success)
                    {
                        AudioClip clip = DownloadHandlerAudioClip.GetContent(audioReq);
                        if (audioSourceVoce != null && clip != null && clip.length > 0f)
                        {
                            audioSourceVoce.clip = clip;
                            audioSourceVoce.Play();
                            Debug.Log($"[TTS] Riproduzione avviata! Durata: {clip.length} sec");
                        }
                        else
                        {
                            Debug.LogError("[TTS Error] Clip non valida o durata zero.");
                        }
                    }
                    else
                    {
                        Debug.LogError($"[Audio Load Error]: {audioReq.error}");
                    }
                }
            }
            else
            {
                Debug.LogError($"[TTS Error]: {req.error}");
            }
        }

        ttsInCorso = false;
    }
}