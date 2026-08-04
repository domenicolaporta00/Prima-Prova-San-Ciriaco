using UnityEngine;

public class Hotspot : MonoBehaviour
{
    [Header("Informazioni Hotspot")]
    public string titolo = "Titolo Attrazione";
    [TextArea(3, 5)]
    public string descrizione = "Inserisci qui la descrizione dell'oggetto...";
}