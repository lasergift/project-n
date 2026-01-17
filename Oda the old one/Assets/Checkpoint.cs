using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerMovement pm = collision.GetComponent<PlayerMovement>();
            if (pm != null)
            {
                // 1. Mise à jour de la position de respawn en mémoire
                pm.UpdateCheckpoint(transform.position);

                // 2. SAUVEGARDE AUTOMATIQUE SUR LE DISQUE
                int currentSlot = PlayerPrefs.GetInt("CurrentSlot", 0);
                
                PlayerData data = new PlayerData();
                data.slotIndex = currentSlot;
                data.lastSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                data.x = transform.position.x;
                data.y = transform.position.y;

                SaveSystem.Save(data, currentSlot);
                Debug.Log("Partie sauvegardée automatiquement sur le Slot " + currentSlot);
            }
        }
    }
}