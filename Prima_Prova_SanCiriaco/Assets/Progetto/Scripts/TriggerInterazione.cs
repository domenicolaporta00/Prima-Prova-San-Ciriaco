using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class TriggerInterazione : MonoBehaviour
{
    [Header("Interfaccia Utente")]
    [Tooltip("Il pannello o testo UI 'Premi E per...' a schermo")]
    [SerializeField] private GameObject promptUI;

    [Header("Azione da eseguire")]
    [Tooltip("La funzione da avviare quando viene premuto E")]
    [SerializeField] private UnityEvent azioneInterazione;

    private bool visitatoreInZona = false;

    void Start()
    {
        // Assicura che il Collider si comporti da volume invisibile attraversabile
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
    }

    void Update()
    {
        if (visitatoreInZona && Input.GetKeyDown(KeyCode.E))
        {
            azioneInterazione?.Invoke();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Rileva l'avatar del giocatore tramite CharacterController o tag MainCamera
        if (other.GetComponent<CharacterController>() != null || other.CompareTag("MainCamera"))
        {
            visitatoreInZona = true;
            if (promptUI != null)
            {
                promptUI.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<CharacterController>() != null || other.CompareTag("MainCamera"))
        {
            visitatoreInZona = false;
            if (promptUI != null)
            {
                promptUI.SetActive(false);
            }
        }
    }
}