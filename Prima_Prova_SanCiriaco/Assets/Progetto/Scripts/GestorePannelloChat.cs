using UnityEngine;
using UnityEngine.UI;

public class GestorePannelloChat : MonoBehaviour
{
    [Header("Riferimenti UI")]
    [Tooltip("Trascina qui il pannello della chat da mostrare/nascondere")]
    public GameObject pannelloChat;

    [Tooltip("Trascina qui il pulsante della 'X' (Bottone_Chiudi)")]
    public Button bottoneChiudi;

    [Header("Controllo Giocatore e Camera")]
    [Tooltip("Trascina qui l'oggetto Player contenente lo script di movimento/camera")]
    public ControlloCamera scriptControlloCamera;

    [Tooltip("Trascina qui l'oggetto CM_PlayerCamera")]
    public GameObject virtualCamera;

    private bool isOpen = false;

    private void Start()
    {
        // Assicura che il cursore sia visibile e libero fin dall'avvio
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Assicura che il pannello sia chiuso all'avvio della scena
        if (pannelloChat != null)
        {
            pannelloChat.SetActive(false);
        }

        // Collega l'evento di chiusura al click del bottone X
        if (bottoneChiudi != null)
        {
            bottoneChiudi.onClick.AddListener(ChiudiChat);
        }
    }

    private void Update()
    {
        // Se la finestra è aperta e l'utente preme ESC, chiudi la chat
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            ChiudiChat();
        }
    }

    /// <summary>
    /// Metodo da richiamare tramite l'evento 'Azione da eseguire' nel trigger
    /// </summary>
    public void ApriChat()
    {
        isOpen = true;

        if (pannelloChat != null)
            pannelloChat.SetActive(true);

        // Mantiene il cursore sbloccato e visibile
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Congela il movimento del Player
        if (scriptControlloCamera != null)
            scriptControlloCamera.enabled = false;

        // Congela Cinemachine
        if (virtualCamera != null)
            virtualCamera.SetActive(false);
    }

    /// <summary>
    /// Metodo richiamato dal bottone X o dal tasto ESC
    /// </summary>
    public void ChiudiChat()
    {
        isOpen = false;

        if (pannelloChat != null)
            pannelloChat.SetActive(false);

        // Mantiene il cursore sempre sbloccato e visibile
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Riattiva il movimento del Player e la telecamera
        if (scriptControlloCamera != null)
            scriptControlloCamera.enabled = true;

        if (virtualCamera != null)
            virtualCamera.SetActive(true);
    }
}