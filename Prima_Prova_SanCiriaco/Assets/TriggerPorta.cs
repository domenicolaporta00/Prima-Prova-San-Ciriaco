using UnityEngine;

public class TriggerPorta : MonoBehaviour
{
    [Header("Interfaccia Utente")]
    [Tooltip("Il pannello UI con il testo 'Premi E per entrare/uscire'")]
    public GameObject pannelloMessaggio;

    [Header("Destinazione Teletrasporto")]
    [Tooltip("L'Empty dove verrà spostata la telecamera premendo E")]
    public Transform puntoDestinazione;

    [Header("Gestione Audio")]
    [Tooltip("Sorgenti audio da fermare quando premi E")]
    public AudioSource[] audioDaDisattivare;

    [Tooltip("Sorgenti audio da far partire quando premi E")]
    public AudioSource[] audioDaAttivare;

    private bool giocatoreInZona = false;
    private Transform visitatoreTransform;

    void Start()
    {
        if (pannelloMessaggio != null)
        {
            pannelloMessaggio.SetActive(false);
        }
    }

    void Update()
    {
        if (giocatoreInZona && Input.GetKeyDown(KeyCode.E))
        {
            EseguiTeletrasporto();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        CharacterController controller = other.GetComponent<CharacterController>();

        if (controller != null)
        {
            giocatoreInZona = true;
            visitatoreTransform = other.transform;

            if (pannelloMessaggio != null)
            {
                pannelloMessaggio.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CharacterController controller = other.GetComponent<CharacterController>();

        if (controller != null)
        {
            giocatoreInZona = false;

            if (pannelloMessaggio != null)
            {
                pannelloMessaggio.SetActive(false);
            }
        }
    }

    private void EseguiTeletrasporto()
    {
        if (visitatoreTransform != null && puntoDestinazione != null)
        {
            if (pannelloMessaggio != null)
            {
                pannelloMessaggio.SetActive(false);
            }

            CharacterController controller = visitatoreTransform.GetComponent<CharacterController>();

            if (controller != null)
            {
                controller.enabled = false;
            }

            // Spostamento posizione e rotazione
            visitatoreTransform.position = puntoDestinazione.position;
            visitatoreTransform.rotation = puntoDestinazione.rotation;

            if (controller != null)
            {
                controller.enabled = true;
            }

            // 1. Spegni le tracce specificate
            if (audioDaDisattivare != null)
            {
                foreach (AudioSource sorgente in audioDaDisattivare)
                {
                    if (sorgente != null) sorgente.Stop();
                }
            }

            // 2. Accendi le nuove tracce specificate
            if (audioDaAttivare != null)
            {
                foreach (AudioSource sorgente in audioDaAttivare)
                {
                    if (sorgente != null) sorgente.Play();
                }
            }

            giocatoreInZona = false;
        }
        else
        {
            Debug.LogError("❌ Errore: Punto Destinazione NON assegnato nell'Inspector!");
        }
    }
}