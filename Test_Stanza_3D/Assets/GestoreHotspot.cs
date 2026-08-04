using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class GestoreHotspot : MonoBehaviour
{
    [Header("Riferimenti UI")]
    public GameObject pannelloInfo;
    public TextMeshProUGUI testoTitolo;
    public TextMeshProUGUI testoDescrizione;

    void Start()
    {
        // Nasconde il pannello all'avvio
        if (pannelloInfo != null)
            pannelloInfo.SetActive(false);
    }

    void Update()
    {
        // Se premi il tasto sinistro del mouse
        if (Input.GetMouseButtonDown(0))
        {
            // Evita di rilevare il click se stai cliccando sui bottoni dell'interfaccia UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Lancia un raggio visivo dalla telecamera verso il punto cliccato
            if (Physics.Raycast(ray, out hit))
            {
                Hotspot hotspot = hit.collider.GetComponent<Hotspot>();
                if (hotspot != null)
                {
                    ApriScheda(hotspot.titolo, hotspot.descrizione);
                }
            }
        }
    }

    public void ApriScheda(string titolo, string descrizione)
    {
        if (pannelloInfo != null)
        {
            testoTitolo.text = titolo;
            testoDescrizione.text = descrizione;
            pannelloInfo.SetActive(true);
        }
    }

    public void ChiudiScheda()
    {
        if (pannelloInfo != null)
            pannelloInfo.SetActive(false);
    }
}