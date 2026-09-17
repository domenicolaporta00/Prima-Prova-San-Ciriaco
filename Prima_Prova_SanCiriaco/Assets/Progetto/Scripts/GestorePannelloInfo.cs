using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GestorePannelloInfo : MonoBehaviour
{
    public static GestorePannelloInfo Instance;

    [Header("Riferimenti Elementi UI")]
    [SerializeField] private TextMeshProUGUI testoTitolo;
    [SerializeField] private TextMeshProUGUI testoDescrizione;
    [SerializeField] private Image immagineFoto;
    [SerializeField] private GameObject pannelloRoot;

    void Awake()
    {
        Instance = this;
        if (pannelloRoot == null) pannelloRoot = gameObject;
    }

    void Start()
    {
        ChiudiScheda();
    }

    public void MostraScheda(string titolo, string descrizione, Sprite foto)
    {
        testoTitolo.text = titolo;
        testoDescrizione.text = descrizione;

        if (foto != null)
        {
            immagineFoto.sprite = foto;
            immagineFoto.gameObject.SetActive(true);
        }
        else
        {
            immagineFoto.gameObject.SetActive(false);
        }

        pannelloRoot.SetActive(true);
    }

    public void ChiudiScheda()
    {
        pannelloRoot.SetActive(false);
    }
}