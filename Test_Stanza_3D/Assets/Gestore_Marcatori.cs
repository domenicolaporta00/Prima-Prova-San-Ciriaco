using UnityEngine;

public class GestoreMarcatore : MonoBehaviour
{
    [Header("Configurazione Prefab")]
    [Tooltip("Trascina qui il Prefab del marcatore che vuoi far comparire")]
    public GameObject marcatorePrefab;

    [Header("Impostazioni Tempo")]
    [Tooltip("Tempo in secondi prima che il marcatore venga distrutto")]
    public float tempoVitaMarcatore = 2.0f;

    void Update()
    {
        // Controlliamo se l'utente clicca con il tasto sinistro del mouse
        if (Input.GetMouseButtonDown(0))
        {
            SparaRaycastEGeneraMarcatore();
        }
    }

    void SparaRaycastEGeneraMarcatore()
    {
        // Creiamo un raggio dalla telecamera verso la posizione del mouse
        Ray raggio = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Se il raggio colpisce un oggetto 3D col Collider
        if (Physics.Raycast(raggio, out hit))
        {
            // Controlliamo che l'oggetto colpito sia un pavimento (interno o esterno)
            if (hit.collider.CompareTag("Pavimento"))
            {
                // 1. INSTANTIATE: Creiamo una copia del marcatore nel punto di impatto del raggio
                // Solleviamo leggermente la posizione Y (+0.05f) per evitare lo sfarfallio col pavimento
                GameObject nuovoMarcatore = Instantiate(marcatorePrefab, hit.point + new Vector3(0, 0.05f, 0), Quaternion.identity);

                // 2. DESTROY: Programmiamo la distruzione dell'oggetto creato dopo tot secondi
                Destroy(nuovoMarcatore, tempoVitaMarcatore);

                Debug.Log("✨ Marcatore creato in posizione: " + hit.point + " | Verrà distrutto tra " + tempoVitaMarcatore + "s");
            }
        }
    }
}