using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class ControlloCamera : MonoBehaviour
{
    [Header("Parametri Movimento")]
    [Tooltip("Velocità di spostamento del visitatore")]
    public float velocitaMovimento = 10.0f;

    [Header("Parametri Visuale Mouse")]
    [Tooltip("Sensibilità del mouse per ruotare la vista")]
    public float sensibilitaMouse = 2.0f;

    [Tooltip("Limite di inclinazione dello sguardo in alto e in basso (gradi)")]
    public float limiteSguardoVerticale = 80.0f;

    [Header("Audio Passi")]
    [Tooltip("Intervallo tra i passi (0.3 - 0.35s consigliato per velocità 10)")]
    [SerializeField] private float intervalloPassi = 0.32f;

    [Tooltip("Clip audio dei passi (puoi inserirne più di una per variare)")]
    [SerializeField] private AudioClip[] suoniPassi;

    private float rotazioneX = 0.0f;
    private float rotazioneY = 0.0f;
    private CharacterController characterController;
    private AudioSource audioSource;
    private float timerPasso = 0.0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();

        // Configura l'AudioSource in modalità corretta
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0.0f; // Audio 2D (sentito in prima persona)
            audioSource.volume = 0.35f;
        }

        SincronizzaAngoli();
    }

    void Update()
    {
        Cursor.visible = true;

        // 1. ROTAZIONE VISUALE (SOLO CON TASTO SINISTRO)
        if (Input.GetMouseButtonDown(0))
        {
            SincronizzaAngoli();
        }

        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X") * sensibilitaMouse;
            float mouseY = Input.GetAxis("Mouse Y") * sensibilitaMouse;

            rotazioneY -= mouseX;
            rotazioneX += mouseY;

            rotazioneX = Mathf.Clamp(rotazioneX, -limiteSguardoVerticale, limiteSguardoVerticale);
            transform.localRotation = Quaternion.Euler(rotazioneX, rotazioneY, 0.0f);
        }

        // 2. SPOSTAMENTO CON COLLISIONI (FRECCE / WASD)
        float orizzontale = Input.GetAxis("Horizontal");
        float verticale = Input.GetAxis("Vertical");

        Vector3 direzioneAvanti = transform.forward;
        Vector3 direzioneDestra = transform.right;

        direzioneAvanti.y = 0;
        direzioneDestra.y = 0;

        direzioneAvanti.Normalize();
        direzioneDestra.Normalize();

        Vector3 direzioneMovimento = direzioneAvanti * verticale + direzioneDestra * orizzontale;
        Vector3 spostamento = direzioneMovimento * velocitaMovimento;

        characterController.Move(spostamento * Time.deltaTime);

        // 3. GESTIONE RUMORE DEI PASSI
        bool siStaMuovendo = direzioneMovimento.sqrMagnitude > 0.01f;

        if (siStaMuovendo)
        {
            timerPasso += Time.deltaTime;

            if (timerPasso >= intervalloPassi)
            {
                RiproduciPasso();
                timerPasso = 0.0f;
            }
        }
        else
        {
            // Ripristina il timer per riprodurre subito il passo alla ripartenza
            timerPasso = intervalloPassi;
        }
    }

    private void RiproduciPasso()
    {
        if (suoniPassi != null && suoniPassi.Length > 0 && audioSource != null)
        {
            AudioClip clipCasuale = suoniPassi[Random.Range(0, suoniPassi.Length)];
            audioSource.pitch = Random.Range(0.92f, 1.08f); // Leggera variazione di tono realistica
            audioSource.PlayOneShot(clipCasuale);
        }
    }

    private void SincronizzaAngoli()
    {
        Vector3 angoli = transform.localRotation.eulerAngles;

        rotazioneX = angoli.x;
        if (rotazioneX > 180.0f)
        {
            rotazioneX -= 360.0f;
        }

        rotazioneY = angoli.y;
        if (rotazioneY > 180.0f)
        {
            rotazioneY -= 360.0f;
        }
    }
}