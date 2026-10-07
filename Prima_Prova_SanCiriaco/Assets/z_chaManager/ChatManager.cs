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

    [TextArea(3, 8)]
    [SerializeField] private string istruzioniSistema = 
        "Sei un assistente virtuale per il sito di San Ciriaco. Rispondi in italiano in modo chiaro e sintetico.";

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
            Debug.LogError("Riferimenti UI mancanti nell'Inspector di ChatManager!");
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
                testoRisposta.text = "Errore di connessione a Ollama. Assicurati che sia avviato in background.";
            }
        }

        inputDomanda.text = "";
        bottoneInvia.interactable = true;
    }
}