using UnityEngine;

public class TriggerPorta : MonoBehaviour
{
    [Header("Interfaccia Utente")]
    [Tooltip("Il pannello UI con il testo 'Premi E per entrare'")]
    public GameObject pannelloMessaggio;

    [Header("Destinazione Teletrasporto")]
    [Tooltip("L'Empty dove verrà spostata la telecamera premendo E")]
    public Transform puntoDestinazione;

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
        // Riconosce il giocatore cercando il CharacterController (indipendentemente dal nome dell'oggetto)
        CharacterController controller = other.GetComponent<CharacterController>();

        if (controller != null)
        {
            giocatoreInZona = true;
            visitatoreTransform = other.transform;

            if (pannelloMessaggio != null)
            {
                pannelloMessaggio.SetActive(true);
            }
            else
            {
                Debug.LogWarning("⚠️ Attenzione: Il Pannello Messaggio non è stato assegnato nell'Inspector!");
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

            // Spostiamo la posizione e la rotazione
            visitatoreTransform.position = puntoDestinazione.position;
            visitatoreTransform.rotation = puntoDestinazione.rotation;

            if (controller != null)
            {
                controller.enabled = true;
            }

            giocatoreInZona = false;
        }
        else
        {
            Debug.LogError("❌ Errore: Punto Destinazione NON assegnato nell'Inspector!");
        }
    }
}