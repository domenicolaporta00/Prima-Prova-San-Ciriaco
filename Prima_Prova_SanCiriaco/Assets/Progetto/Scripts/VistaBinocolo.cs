using UnityEngine;
using UnityEngine.UI;

public class VistaBinocolo : MonoBehaviour
{
    [Header("Riferimenti UI")]
    public GameObject pannelloBinocolo;
    public ScrollRect scrollRect;
    public RectTransform fotoPanorama;
    public Button bottoneChiudi;

    [Header("Velocita e Movimento")]
    public float velocitaTastiera = 0.5f;

    [Header("Parametri Zoom")]
    public float sensibilitaRotellina = 3f;
    public float fluiditaZoom = 7f;
    public float minZoom = 1.0f;
    public float maxZoom = 2.5f;

    private float targetZoom = 1.0f;
    private float zoomAttuale = 1.0f;

    [Header("Controllo Giocatore e Camera")]
    [Tooltip("Trascina qui l'oggetto Player")]
    public ControlloCamera scriptControlloCamera; 

    [Tooltip("Trascina qui l'oggetto CM_PlayerCamera")]
    public GameObject virtualCamera;

    private bool isOpen = false;

    void Start()
    {
        if (pannelloBinocolo != null)
            pannelloBinocolo.SetActive(false);

        if (bottoneChiudi != null)
            bottoneChiudi.onClick.AddListener(ChiudiBinocolo);
    }

    void Update()
    {
        if (!isOpen) return;

        // 1. GESTIONE ZOOM MORBIDO
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            targetZoom += scroll * sensibilitaRotellina;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        zoomAttuale = Mathf.Lerp(zoomAttuale, targetZoom, Time.deltaTime * fluiditaZoom);

        if (fotoPanorama != null)
        {
            fotoPanorama.localScale = new Vector3(zoomAttuale, zoomAttuale, 1f);
        }

        // 2. SCORRIMENTO ORIZZONTALE (A/D o Frecce SX/DX)
        float inputOrizzontale = Input.GetAxis("Horizontal");
        if (Mathf.Abs(inputOrizzontale) > 0.01f)
        {
            scrollRect.horizontalNormalizedPosition += inputOrizzontale * velocitaTastiera * Time.deltaTime;
            scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(scrollRect.horizontalNormalizedPosition);
        }

        // 3. SCORRIMENTO VERTICALE (W/S o Frecce SU/GIU)
        float inputVerticale = Input.GetAxis("Vertical");
        if (Mathf.Abs(inputVerticale) > 0.01f)
        {
            scrollRect.verticalNormalizedPosition += inputVerticale * velocitaTastiera * Time.deltaTime;
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
        }

        // 4. CHIUSURA CON ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ChiudiBinocolo();
        }
    }

    public void ApriBinocolo()
    {
        isOpen = true;
        pannelloBinocolo.SetActive(true);

        // Reset zoom e centratura
        targetZoom = 1.0f;
        zoomAttuale = 1.0f;

        if (fotoPanorama != null)
            fotoPanorama.localScale = Vector3.one;

        scrollRect.horizontalNormalizedPosition = 0.5f;
        scrollRect.verticalNormalizedPosition = 0.5f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Congela il movimento del Player e la visuale del mouse
        if (scriptControlloCamera != null)
            scriptControlloCamera.enabled = false;

        // Disattiva Cinemachine per congelare completamente qualsiasi oscillazione 3D
        if (virtualCamera != null)
            virtualCamera.SetActive(false);
    }

    public void ChiudiBinocolo()
    {
        isOpen = false;
        pannelloBinocolo.SetActive(false);

        // Riattiva il Player
        if (scriptControlloCamera != null)
            scriptControlloCamera.enabled = true;

        // Riattiva Cinemachine
        if (virtualCamera != null)
            virtualCamera.SetActive(true);
    }
}