using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ControlloCamera : MonoBehaviour
{
    [Header("Parametri Movimento")]
    [Tooltip("Velocità di spostamento del visitatore")]
    public float velocitaMovimento = 5.0f;

    [Header("Parametri Visuale Mouse")]
    [Tooltip("Sensibilità del mouse per ruotare la vista")]
    public float sensibilitaMouse = 2.0f;

    [Tooltip("Limite di inclinazione dello sguardo in alto e in basso (gradi)")]
    public float limiteSguardoVerticale = 80.0f;

    private float rotazioneX = 0.0f;
    private float rotazioneY = 0.0f;
    private CharacterController characterController;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        characterController = GetComponent<CharacterController>();

        // Sincronizziamo la rotazione iniziale
        SincronizzaAngoli();
    }

    void Update()
    {
        Cursor.visible = true;

        // ==========================================
        // 1. ROTAZIONE VISUALE (SOLO CON TASTO SINISTRO)
        // ==========================================
        
        // APPENA SI CLICCA: Rialliniamo le variabili con la rotazione reale della telecamera
        if (Input.GetMouseButtonDown(0))
        {
            SincronizzaAngoli();
        }

        // MENTRE SI TIENE PREMUTO: Applichiamo la rotazione fluida
        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X") * sensibilitaMouse;
            float mouseY = Input.GetAxis("Mouse Y") * sensibilitaMouse;

            rotazioneY -= mouseX;
            rotazioneX += mouseY;

            rotazioneX = Mathf.Clamp(rotazioneX, -limiteSguardoVerticale, limiteSguardoVerticale);
            transform.localRotation = Quaternion.Euler(rotazioneX, rotazioneY, 0.0f);
        }

        // ==========================================
        // 2. SPOSTAMENTO CON COLLISIONI (FRECCE / WASD)
        // ==========================================
        float orizzontale = Input.GetAxis("Horizontal");
        float verticale = Input.GetAxis("Vertical");

        Vector3 direzioneAvanti = transform.forward;
        Vector3 direzioneDestra = transform.right;

        direzioneAvanti.y = 0;
        direzioneDestra.y = 0;

        direzioneAvanti.Normalize();
        direzioneDestra.Normalize();

        Vector3 spostamento = (direzioneAvanti * verticale + direzioneDestra * orizzontale) * velocitaMovimento;

        characterController.Move(spostamento * Time.deltaTime);
    }

    /// <summary>
    /// Legge la rotazione attuale della telecamera e aggiorna le variabili per evitare scatti
    /// </summary>
    private void SincronizzaAngoli()
    {
        Vector3 angoli = transform.localRotation.eulerAngles;

        // Gestiamo il passaggio da 0-360 gradi al range con segno (-180 a +180) per la pendenza (pitch)
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