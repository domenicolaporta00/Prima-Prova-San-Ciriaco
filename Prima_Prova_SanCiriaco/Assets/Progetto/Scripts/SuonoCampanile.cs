using System.Collections;
using UnityEngine;

public class SuonoCampanile : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private float intervalloSecondi = 60f; // Tempo di attesa tra i rintocchi

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(RoutineCampane());
    }

    private IEnumerator RoutineCampane()
    {
        while (true)
        {
            yield return new WaitForSeconds(intervalloSecondi);
            
            if (audioSource != null && audioSource.clip != null)
            {
                audioSource.Play();
            }
        }
    }
}