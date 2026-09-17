using UnityEngine;
using UnityEngine.UI;

public class VistaBinocolo : MonoBehaviour
{
    [Header("Riferimenti UI")]
    public GameObject pannelloBinocolo;
    public ScrollRect scrollRect;
    public Button bottoneChiudi;

    [Header("Velocita Scorrimento Tastiera")]
    public float velocitaTastiera = 0.5f;

    [Header("Controllo Giocatore")]
    public ControlloCamera scriptControlloCamera; 

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

        // Scorrimento orizzontale (A/D o Frecce SX/DX)
        float inputOrizzontale = Input.GetAxis("Horizontal");
        if (Mathf.Abs(inputOrizzontale) > 0.01f)
        {
            scrollRect.horizontalNormalizedPosition += inputOrizzontale * velocitaTastiera * Time.deltaTime;
            scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(scrollRect.horizontalNormalizedPosition);
        }

        // Scorrimento verticale (W/S o Frecce SU/GIU)
        float inputVerticale = Input.GetAxis("Vertical");
        if (Mathf.Abs(inputVerticale) > 0.01f)
        {
            scrollRect.verticalNormalizedPosition += inputVerticale * velocitaTastiera * Time.deltaTime;
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
        }

        // Chiusura rapida con ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ChiudiBinocolo();
        }
    }

    public void ApriBinocolo()
    {
        isOpen = true;
        pannelloBinocolo.SetActive(true);

        // Centra la vista sia in orizzontale che in verticale all'apertura
        scrollRect.horizontalNormalizedPosition = 0.5f;
        scrollRect.verticalNormalizedPosition = 0.5f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (scriptControlloCamera != null)
            scriptControlloCamera.enabled = false;
    }

    public void ChiudiBinocolo()
    {
        isOpen = false;
        pannelloBinocolo.SetActive(false);

        if (scriptControlloCamera != null)
            scriptControlloCamera.enabled = true;
    }
}