using UnityEngine;

public class HotspotQuadro : MonoBehaviour
{
    [Header("Interfaccia Utente")]
    [Tooltip("Il pannello UI che contiene le informazioni del quadro")]
    public GameObject pannelloInfo;

    [Header("Distanza di Interazione")]
    [Tooltip("Distanza massima da cui il visitatore può cliccare il quadro (in metri)")]
    public float distanzaMassima = 5.0f;

    void Start()
    {
        // Ci assicuriamo che il pannello sia chiuso all'avvio
        if (pannelloInfo != null)
        {
            pannelloInfo.SetActive(false);
        }
    }

    void Update()
    {
        // Se il visitatore fa clic con il tasto sinistro
        if (Input.GetMouseButtonDown(0))
        {
            Ray raggio = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Inviamo un Raycast dal mouse verso la scena
            if (Physics.Raycast(raggio, out hit, distanzaMassima))
            {
                // Se il raggio colpisce PROPRIO QUESTO quadro
                if (hit.transform == transform)
                {
                    ApriPannello();
                }
            }
        }
    }

    public void ApriPannello()
    {
        if (pannelloInfo != null)
        {
            pannelloInfo.SetActive(true);
        }
    }

    // Funzione usata dal bottone "X" per chiudere la scheda
    public void ChiudiPannello()
    {
        if (pannelloInfo != null)
        {
            pannelloInfo.SetActive(false);
        }
    }
}