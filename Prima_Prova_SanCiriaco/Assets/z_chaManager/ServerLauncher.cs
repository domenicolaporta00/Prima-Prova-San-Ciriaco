using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class ServerLauncher : MonoBehaviour
{
    [Header("Server")]
    [SerializeField] private string urlHealth = "http://127.0.0.1:5000/health";
    [SerializeField] private float timeoutSecondi = 120f;

    [Header("Schermata di caricamento")]
    [SerializeField] private bool mostraCaricamento = true;
    [SerializeField] private Sprite immagineCaricamento;
    [SerializeField] private Vector2 dimensioneImmagine = new Vector2(1000, 450);
    [SerializeField] private string testoMessaggio = "Preparazione scena";
    [SerializeField] private int dimensioneTesto = 44;
    [SerializeField, Range(0f, 1f)] private float opacitaSfondo = 0.92f;

    [Header("Pulsazione del logo")]
    [Tooltip("Opacità minima del logo (0 = sparisce del tutto)")]
    [SerializeField, Range(0f, 1f)] private float opacitaMinima = 0.25f;
    [Tooltip("Secondi per un ciclo completo (chiaro -> scuro -> chiaro)")]
    [SerializeField] private float durataCiclo = 2.2f;

    [Header("Da bloccare finché il server non è pronto")]
    [SerializeField] private Selectable[] daBloccare;
    [SerializeField] private Behaviour[] componentiDaDisabilitare;

    public bool ServerPronto { get; private set; }

    private Process processo;

    private GameObject overlay;
    private CanvasGroup gruppoOverlay;
    private CanvasGroup gruppoLogo;
    private TextMeshProUGUI testoCaricamento;

    private string PercorsoBat =>
        Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Server", "avvia_server.bat"));

    private IEnumerator Start()
    {
        // Overlay e blocco subito, prima del primo frame: niente flash della scena
        Blocca(true);
        if (mostraCaricamento) CreaOverlay();

        // 1. Server già attivo? Allora toglie subito la schermata
        bool attivo = false;
        yield return ControllaHealth(r => attivo = r);
        if (attivo)
        {
            ServerPronto = true;
            UnityEngine.Debug.Log("[Launcher] Server già attivo.");
            Blocca(false);
            yield return ChiudiOverlay(null, 0f);
            yield break;
        }

        string bat = PercorsoBat;
        if (!File.Exists(bat))
        {
            UnityEngine.Debug.LogError($"[Launcher] File non trovato: {bat}");
            Blocca(false);
            yield return ChiudiOverlay("File di avvio non trovato", 2f);
            yield break;
        }

        // 3. Avvio senza finestra
        bool avvioFallito = false;
        try
        {
            processo = Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"\"{bat}\" silent\"",
                WorkingDirectory = Path.GetDirectoryName(bat),
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            });
            UnityEngine.Debug.Log($"[Launcher] Script di avvio lanciato (nascosto): {bat}");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"[Launcher] Impossibile avviare lo script: {e.Message}");
            avvioFallito = true;
        }

        if (avvioFallito)
        {
            yield return ChiudiOverlay("Errore di avvio del server", 3f);
            Blocca(false);
            yield break;
        }

        // 4. Attesa: animazione ogni frame, health ogni 3 secondi
        float inizio = Time.unscaledTime;
        float prossimoControllo = inizio + 3f;
        bool controlloInCorso = false;

        while (Time.unscaledTime - inizio < timeoutSecondi)
        {
            if (!controlloInCorso && Time.unscaledTime >= prossimoControllo)
            {
                controlloInCorso = true;
                StartCoroutine(ControllaHealth(ok =>
                {
                    if (ok) ServerPronto = true;
                    controlloInCorso = false;
                    prossimoControllo = Time.unscaledTime + 3f;
                }));
            }

            if (ServerPronto) break;

            Anima(Time.unscaledTime - inizio);
            yield return null;
        }

        if (ServerPronto)
        {
            UnityEngine.Debug.Log($"[Launcher] Server pronto dopo {Time.unscaledTime - inizio:F0}s.");
            Blocca(false);
            yield return ChiudiOverlay(null, 0f);
        }
        else
        {
            UnityEngine.Debug.LogError("[Launcher] Timeout: il server non è partito. Controlla Server\\server.log");
            yield return ChiudiOverlay("Server non raggiungibile (vedi server.log)", 3f);
            Blocca(false);
        }
    }

    // ------------------------------------------------------------------
    private IEnumerator ControllaHealth(Action<bool> risultato)
    {
        using (UnityWebRequest req = UnityWebRequest.Get(urlHealth))
        {
            req.timeout = 2;
            yield return req.SendWebRequest();
            risultato?.Invoke(req.result == UnityWebRequest.Result.Success);
        }
    }

    // ------------------------------------------------------------------
    // Overlay
    // ------------------------------------------------------------------
    private void CreaOverlay()
    {
        overlay = new GameObject("CaricamentoServer");
        Canvas canvas = overlay.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        CanvasScaler scaler = overlay.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        overlay.AddComponent<GraphicRaycaster>();
        gruppoOverlay = overlay.AddComponent<CanvasGroup>();

        // Sfondo
        Image sfondo = CreaImage("Sfondo", overlay.transform, new Color(0f, 0f, 0f, opacitaSfondo));
        Stira(sfondo.rectTransform);

        // Logo (con CanvasGroup per la pulsazione)
        float yTesto = -dimensioneImmagine.y / 2f - 30f;
        if (immagineCaricamento != null)
        {
            Image logo = CreaImage("Logo", overlay.transform, Color.white);
            logo.sprite = immagineCaricamento;
            logo.preserveAspect = true;
            logo.raycastTarget = false;
            gruppoLogo = logo.gameObject.AddComponent<CanvasGroup>();

            RectTransform rl = logo.rectTransform;
            rl.anchorMin = rl.anchorMax = new Vector2(0.5f, 0.5f);
            rl.sizeDelta = dimensioneImmagine;
            rl.anchoredPosition = new Vector2(0, 40f);
        }
        else
        {
            yTesto = 0f;
        }

        // Testo sotto il logo
        GameObject goTesto = new GameObject("Testo", typeof(RectTransform));
        goTesto.transform.SetParent(overlay.transform, false);
        testoCaricamento = goTesto.AddComponent<TextMeshProUGUI>();
        testoCaricamento.alignment = TextAlignmentOptions.Center;
        testoCaricamento.fontSize = dimensioneTesto;
        testoCaricamento.color = Color.white;
        testoCaricamento.raycastTarget = false;
        testoCaricamento.text = testoMessaggio;

        RectTransform rt = testoCaricamento.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(1400, 100);
        rt.anchoredPosition = new Vector2(0, 40f + yTesto);
    }

    private void Anima(float secondi)
    {
        if (overlay == null) return;

        // Pulsazione: oscilla tra opacitaMinima e 1 con andamento morbido (seno)
        if (gruppoLogo != null)
        {
            float onda = (Mathf.Sin(secondi * 2f * Mathf.PI / Mathf.Max(0.1f, durataCiclo) - Mathf.PI / 2f) + 1f) / 2f;
            gruppoLogo.alpha = Mathf.Lerp(opacitaMinima, 1f, onda);
        }

        // Puntini: quelli non ancora "accesi" restano invisibili, così la scritta non si sposta
        int punti = (int)(secondi * 2f) % 4;
        string accesi = new string('.', punti);
        string spenti = new string('.', 3 - punti);
        testoCaricamento.text = $"{testoMessaggio}{accesi}<alpha=#00>{spenti}";
    }

    private IEnumerator ChiudiOverlay(string messaggio, float attesa)
    {
        if (overlay == null) yield break;

        if (!string.IsNullOrEmpty(messaggio))
        {
            testoCaricamento.text = messaggio;
            if (gruppoLogo != null) gruppoLogo.alpha = 1f;
            yield return new WaitForSecondsRealtime(attesa);
        }

        // Dissolvenza finale
        float durata = 0.6f;
        float t = 0f;
        while (t < durata && gruppoOverlay != null)
        {
            t += Time.unscaledDeltaTime;
            gruppoOverlay.alpha = 1f - Mathf.Clamp01(t / durata);
            yield return null;
        }

        if (overlay != null) Destroy(overlay);
    }

    private static Image CreaImage(string nome, Transform parent, Color colore)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = colore;
        return img;
    }

    private static void Stira(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private void Blocca(bool bloccati)
    {
        if (daBloccare != null)
            foreach (Selectable s in daBloccare)
                if (s != null) s.interactable = !bloccati;

        if (componentiDaDisabilitare != null)
            foreach (Behaviour b in componentiDaDisabilitare)
                if (b != null) b.enabled = !bloccati;
    }

    private void OnApplicationQuit()
    {
        if (processo != null && !processo.HasExited)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "taskkill",
                Arguments = $"/PID {processo.Id} /T /F",
                CreateNoWindow = true,
                UseShellExecute = false
            });
        }
    }
}