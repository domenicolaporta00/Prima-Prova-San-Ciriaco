using UnityEngine;

public class TestCicloVita : MonoBehaviour
{
    void Awake()
    {
        Debug.Log("1. [Awake] Oggetto caricato in memoria!");
    }

    void OnEnable()
    {
        Debug.Log("2. [OnEnable] Componente ATTIVATO!");
    }

    void Start()
    {
        Debug.Log("3. [Start] Gioco iniziato!");
    }

    void OnDisable()
    {
        Debug.Log("4. [OnDisable] Componente DISATTIVATO!");
    }
}