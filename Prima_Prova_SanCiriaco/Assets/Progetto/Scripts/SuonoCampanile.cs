using System.Collections;
using UnityEngine;

public class SuonoCampanile : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private float intervalloSecondi = 60f;

    [Header("Riferimenti Animator Campane")]
    [SerializeField] private Animator animatorDestra;
    [SerializeField] private Animator animatorSinistra;

    // Memorizzano la rotazione a riposo (0 gradi)
    private Quaternion rotazioneRiposoDx;
    private Quaternion rotazioneRiposoSx;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // Salva la posizione iniziale neutra
        if (animatorDestra != null)
        {
            rotazioneRiposoDx = animatorDestra.transform.localRotation;
            animatorDestra.enabled = false;
        }

        if (animatorSinistra != null)
        {
            rotazioneRiposoSx = animatorSinistra.transform.localRotation;
            animatorSinistra.enabled = false;
        }

        StartCoroutine(RoutineCampane());
    }

    private IEnumerator RoutineCampane()
    {
        while (true)
        {
            yield return new WaitForSeconds(intervalloSecondi);
            
            if (audioSource != null && audioSource.clip != null)
            {
                // Avvia suono e animazione
                audioSource.Play();
                if (animatorDestra != null) animatorDestra.enabled = true;
                if (animatorSinistra != null) animatorSinistra.enabled = true;

                // Attende la fine del suono
                yield return new WaitForSeconds(audioSource.clip.length);

                // Disattiva e riporta dolcemente a 0°
                if (animatorDestra != null)
                {
                    animatorDestra.enabled = false;
                    StartCoroutine(RipristinaPosizione(animatorDestra.transform, rotazioneRiposoDx));
                }

                if (animatorSinistra != null)
                {
                    animatorSinistra.enabled = false;
                    StartCoroutine(RipristinaPosizione(animatorSinistra.transform, rotazioneRiposoSx));
                }
            }
        }
    }

    // Coroutine per evitare uno scatto secco e riallineare la campana al centro
    private IEnumerator RipristinaPosizione(Transform target, Quaternion rotazioneTarget)
    {
        float tempo = 0f;
        float durata = 0.5f;
        Quaternion rotazioneCorrente = target.localRotation;

        while (tempo < durata)
        {
            target.localRotation = Quaternion.Slerp(rotazioneCorrente, rotazioneTarget, tempo / durata);
            tempo += Time.deltaTime;
            yield return null;
        }

        target.localRotation = rotazioneTarget;
    }
}