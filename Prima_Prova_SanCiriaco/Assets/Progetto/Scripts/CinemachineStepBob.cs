using UnityEngine;
using Unity.Cinemachine;

public class CinemachineStepBob : MonoBehaviour
{
    [Header("Parametri Camminata")]
    [Tooltip("Ampiezza del passo in movimento")]
    public float ampiezzaCamminata = 2.0f;
    [Tooltip("Frequenza/cadenza del passo")]
    public float frequenzaCamminata = 1.0f;

    [Header("Parametri Respiro da Fermo")]
    [Tooltip("Ampiezza del respiro quando si è immobili")]
    public float ampiezzaRespiro = 0.12f;
    [Tooltip("Frequenza lenta tipica del respiro calmo")]
    public float frequenzaRespiro = 0.4f;

    [Header("Transizione")]
    [Tooltip("Fluidità del passaggio tra camminata e respiro")]
    public float velocitaTransizione = 4.0f;

    [Header("Riferimenti")]
    [SerializeField] private ControlloCamera controlloCamera;

    private CinemachineBasicMultiChannelPerlin noise;

    void Start()
    {
        noise = GetComponent<CinemachineBasicMultiChannelPerlin>();
        if (controlloCamera == null)
            controlloCamera = GetComponentInParent<ControlloCamera>();
    }

    void Update()
    {
        if (noise == null) return;

        bool puoCamminare = (controlloCamera == null || controlloCamera.enabled);

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool staCamminando = puoCamminare && (Mathf.Abs(h) > 0.05f || Mathf.Abs(v) > 0.05f);

        // Seleziona i target: ritmo rapido e marcato in camminata, lento e impercettibile da fermo
        float targetAmpiezza = staCamminando ? ampiezzaCamminata : ampiezzaRespiro;
        float targetFrequenza = staCamminando ? frequenzaCamminata : frequenzaRespiro;

        // Se il binocolo è aperto, blocca totalmente anche il respiro
        if (!puoCamminare)
        {
            targetAmpiezza = 0f;
        }

        // Interpolazione morbida
        noise.AmplitudeGain = Mathf.Lerp(noise.AmplitudeGain, targetAmpiezza, Time.deltaTime * velocitaTransizione);
        noise.FrequencyGain = Mathf.Lerp(noise.FrequencyGain, targetFrequenza, Time.deltaTime * velocitaTransizione);
    }
}