using UnityEngine;

public class DatiHotspot : MonoBehaviour
{
    [Header("Contenuto Informativo")]
    public string titolo;
    [TextArea(4, 10)] public string descrizione;
    public Sprite fotoDettagliata;

    public void ApriSchedaInformativa()
    {
        if (GestorePannelloInfo.Instance != null)
        {
            GestorePannelloInfo.Instance.MostraScheda(titolo, descrizione, fotoDettagliata);
        }
    }
}