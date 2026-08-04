using UnityEngine;
using TMPro;

public class ControlloCamera : MonoBehaviour
{
    [Header("Sensibilità e Velocità")]
    public float sensibilitaMouse = 3.0f;
    public float velocitaRotazioneTastiera = 100.0f; // Velocità di rotazione con A/D o Frecce
    public float velocitaSpostamento = 5.0f;
    public float altezzaOcchi = 1.6f;

    [Header("Interazione Porta")]
    public Transform portaEsterno;   // Il riferimento fuori dalla porta
    public Transform portaInterno;   // Il riferimento dentro la porta
    public Vector3 puntoArrivoInterno = new Vector3(0f, 1.6f, 2.5f); // Posizione dentro
    public Vector3 puntoArrivoEsterno = new Vector3(0f, 1.6f, -0.5f); // Posizione fuori
    public float distanzaInterazione = 2.0f;

    [Header("Interfaccia Utente (UI)")]
    public GameObject pannelloMessaggio;   
    public TextMeshProUGUI testoMessaggio; 

    private float rotazioneX = 0.0f;
    private float rotazioneY = 0.0f;
    private CharacterController controller;
    private bool dentroCasa = false;

    void Start()
    {
        Vector3 rotazioneIniziale = transform.localRotation.eulerAngles;
        rotazioneX = rotazioneIniziale.y;
        rotazioneY = rotazioneIniziale.x;

        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            controller = gameObject.AddComponent<CharacterController>();
        }
        controller.height = altezzaOcchi;
        controller.center = new Vector3(0, altezzaOcchi / 2f, 0);

        if (pannelloMessaggio != null)
            pannelloMessaggio.SetActive(false);
    }

    void Update()
    {
        // 1. ROTAZIONE SGUARDO CON TASTIERA (A/D o Frecce Sinistra/Destra)
        float orizzontale = Input.GetAxis("Horizontal");
        if (orizzontale != 0.0f)
        {
            rotazioneX += orizzontale * velocitaRotazioneTastiera * Time.deltaTime;
        }

        // 2. ROTAZIONE SGUARDO CON MOUSE (Tenendo premuto il tasto sinistro)
        if (Input.GetMouseButton(0))
        {
            rotazioneX -= Input.GetAxis("Mouse X") * sensibilitaMouse;
            rotazioneY += Input.GetAxis("Mouse Y") * sensibilitaMouse;
            rotazioneY = Mathf.Clamp(rotazioneY, -80f, 80f);
        }

        // Applica la rotazione aggiornata
        transform.localRotation = Quaternion.Euler(rotazioneY, rotazioneX, 0);

        // 3. MOVIMENTO AVANTI / INDIETRO (W/S o Frecce Su/Giù)
        float verticale = Input.GetAxis("Vertical");

        if (verticale != 0.0f)
        {
            Vector3 avanti = transform.forward;
            avanti.y = 0;
            avanti.Normalize();

            Vector3 direzione = avanti * verticale;

            if (controller != null && direzione.magnitude > 0.1f)
            {
                controller.Move(direzione * velocitaSpostamento * Time.deltaTime);
            }
        }

        // 4. CONTROLLO DISTANZA DALLA PORTA ED ESIBIZIONE UI
        GestisciInterazionePorta();
    }

    void GestisciInterazionePorta()
    {
        bool vicinoAQualcosa = false;

        if (!dentroCasa && portaEsterno != null)
        {
            float dist = Vector3.Distance(transform.position, portaEsterno.position);
            if (dist <= distanzaInterazione)
            {
                vicinoAQualcosa = true;
                MostraMessaggio("Premi E per entrare");

                if (Input.GetKeyDown(KeyCode.E))
                {
                    Teletrasporta(puntoArrivoInterno);
                    dentroCasa = true;
                    NascondiMessaggio();
                }
            }
        }
        else if (dentroCasa && portaInterno != null)
        {
            float dist = Vector3.Distance(transform.position, portaInterno.position);
            if (dist <= distanzaInterazione)
            {
                vicinoAQualcosa = true;
                MostraMessaggio("Premi E per uscire");

                if (Input.GetKeyDown(KeyCode.E))
                {
                    Teletrasporta(puntoArrivoEsterno);
                    dentroCasa = false;
                    NascondiMessaggio();
                }
            }
        }

        if (!vicinoAQualcosa)
        {
            NascondiMessaggio();
        }
    }

    void Teletrasporta(Vector3 nuovaPosizione)
    {
        controller.enabled = false;
        transform.position = nuovaPosizione;
        controller.enabled = true;
    }

    void MostraMessaggio(string testo)
    {
        if (pannelloMessaggio != null) pannelloMessaggio.SetActive(true);
        if (testoMessaggio != null) testoMessaggio.text = testo;
    }

    void NascondiMessaggio()
    {
        if (pannelloMessaggio != null) pannelloMessaggio.SetActive(false);
    }
}