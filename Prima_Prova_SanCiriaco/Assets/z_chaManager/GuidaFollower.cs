using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GuidaFollower : MonoBehaviour
{
    [Header("Riferimenti")]
    [SerializeField] private Transform bersaglioGiocatore;
    [SerializeField] private Animator animatore;
    [SerializeField] private GameObject pannelloChat; // Il pannello UI con microfono

    [Header("Distanze e Movimento")]
    [SerializeField] private float distanzaArresto = 2.5f;
    [SerializeField] private float velocitaRotazioneDialogo = 5f;

    private NavMeshAgent agente;
    private bool inDialogo = false;

    private void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
        if (animatore == null) animatore = GetComponent<Animator>();
        
        // Imposta la distanza minima per non collidere con il player
        agente.stoppingDistance = distanzaArresto;
    }

    private void Update()
    {
        if (bersaglioGiocatore == null) return;

        float distanza = Vector3.Distance(transform.position, bersaglioGiocatore.position);

        // 1. Gestione del pulsante di interazione (Tasto E)
        if (distanza <= distanzaArresto + 1.0f && Input.GetKeyDown(KeyCode.E))
        {
            AlternaDialogo();
        }

        // 2. Comportamento durante la conversazione
        if (inDialogo)
        {
            agente.isStopped = true;
            RuotaVersoGiocatore();
            AggiornaAnimazioni(0f);
            return;
        }

        // 3. Inseguimento del giocatore lungo il percorso
        agente.isStopped = false;
        agente.SetDestination(bersaglioGiocatore.position);

        AggiornaAnimazioni(agente.velocity.magnitude);
    }

    private void AlternaDialogo()
    {
        inDialogo = !inDialogo;
        if (pannelloChat != null)
        {
            pannelloChat.SetActive(inDialogo);
        }
    }

    private void RuotaVersoGiocatore()
    {
        Vector3 direzione = (bersaglioGiocatore.position - transform.position).normalized;
        direzione.y = 0; // Evita inclinazioni lungo l'asse verticale
        if (direzione != Vector3.zero)
        {
            Quaternion rotazioneObiettivo = Quaternion.LookRotation(direzione);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotazioneObiettivo, Time.deltaTime * velocitaRotazioneDialogo);
        }
    }

    private void AggiornaAnimazioni(float velocita)
    {
        if (animatore != null)
        {
            animatore.SetFloat("Speed", velocita);
        }
    }
}