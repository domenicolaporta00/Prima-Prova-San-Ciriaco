using System;
using System.Collections;
using System.Text;
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

    [Header("Configurazione Ollama")]
    [SerializeField] private string urlOllama = "http://localhost:11434/api/chat";
    [SerializeField] private string nomeModello = "llama3.2:latest";

    [TextArea(5, 12)]
    [SerializeField] private string istruzioniSistema = 
        "Sei la guida virtuale del Duomo di San Ciriaco (Cattedrale di Ancona) e del suo piazzale sul Colle Guasco.\n" +
        "Rispondi sempre in italiano, in modo accogliente, chiaro e sintetico (massimo 2-3 frasi per risposta).\n\n" +
        "Fatti chiave:\n" +
        "- Posizione: Colle Guasco, vista panoramica sul mare e porto di Ancona.\n" +
        "- Origini: sorge sull'antico tempio greco di Afrodite Euplea.\n" +
        "- Architettura: stile romanico con influssi bizantini, pianta a croce greca.\n" +
        "- Portale: protiro sostenuto dai caratteristici leoni in marmo rosso di Verona.\n" +
        "- Cripta: custodisce le reliquie del patrono San Ciriaco.\n" +
        "- Cupola: a pianta dodecagonale tra le più antiche d'Italia.\n\n" +
        "Se la domanda non riguarda la cattedrale, il piazzale o la sua storia, rispondi cortesemente che sei una guida specializzata solo su San Ciriaco.";

    [Serializable]
    private class Messaggio
    {
        public string role;
        public string content;
    }

    [Serializable]
    private class RichiestaChat
    {
        public string model;
        public Messaggio[] messages;
        public bool stream;
    }

    [Serializable]
    private class RispostaOllama
    {
        public Messaggio message;
        public bool done;
    }

    private void Start()
    {
        if (bottoneInvia != null)
        {
            bottoneInvia.onClick.AddListener(InviaDomanda);
        }
    }

    public void InviaDomanda()
    {
        if (inputDomanda == null || testoRisposta == null)
        {
            Debug.LogError("[ChatManager] Assegna tutti i componenti UI nell'Inspector!");
            return;
        }

        string prompt = inputDomanda.text.Trim();
        if (string.IsNullOrEmpty(prompt)) return;

        bottoneInvia.interactable = false;
        testoRisposta.text = "Sto pensando...";

        StartCoroutine(ChiamaOllamaCoroutine(prompt));
    }

    private IEnumerator ChiamaOllamaCoroutine(string promptUtente)
    {
        RichiestaChat payload = new RichiestaChat
        {
            model = nomeModello,
            stream = false,
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

            if (richiesta.result == UnityWebRequest.Result.Success)
            {
                RispostaOllama risposta = JsonUtility.FromJson<RispostaOllama>(richiesta.downloadHandler.text);
                testoRisposta.text = risposta?.message?.content ?? "Nessuna risposta ricevuta.";
            }
            else
            {
                Debug.LogError($"[Ollama Error]: {richiesta.error}\n{richiesta.downloadHandler.text}");
                testoRisposta.text = "Errore di connessione a Ollama. Verifica che il servizio sia attivo.";
            }
        }

        inputDomanda.text = "";
        bottoneInvia.interactable = true;
    }
}