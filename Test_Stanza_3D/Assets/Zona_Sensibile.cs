using UnityEngine;

public class ZonaSensibile : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Controlliamo se l'oggetto che entra si chiama esattamente "Main Camera"
        if (other.gameObject.name == "Main Camera")
        {
            Debug.Log("➡️ Il VISITATORE si è avvicinato all'opera!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Stesso controllo per quando esce dalla zona
        if (other.gameObject.name == "Main Camera")
        {
            Debug.Log("⬅️ Il VISITATORE si è allontanato dall'opera!");
        }
    }
}