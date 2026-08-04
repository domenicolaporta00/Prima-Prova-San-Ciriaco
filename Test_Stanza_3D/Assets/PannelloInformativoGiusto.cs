using UnityEngine;

public class PannelloInformativoGiusto : MonoBehaviour
{
    private Transform telecamera;

    void Start()
    {
        // Troviamo automaticamente la telecamera principale all'avvio
        if (Camera.main != null)
        {
            telecamera = Camera.main.transform;
        }
    }

    // Usiamo LateUpdate per assicurarci che la telecamera si sia già mossa nel frame attuale
    void LateUpdate()
    {
        if (telecamera == null) return;

        // 1. Calcoliamo la direzione dal pannello verso la telecamera
        Vector3 direzione = telecamera.position - transform.position;

        // 2. Usiamo Quaternion per trasformare il vettore di direzione in una rotazione 3D
        // Usiamo il segno "-" (-direzione) per la UI in modo che il testo non risulti specchiato
        Quaternion rotazioneTarget = Quaternion.LookRotation(-direzione);

        // 3. Applichiamo la rotazione all'oggetto
        transform.rotation = rotazioneTarget;
    }
}
