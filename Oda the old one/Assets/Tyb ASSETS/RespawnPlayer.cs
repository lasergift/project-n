using UnityEngine;
using System.Collections;

public class RespawnPlayer : MonoBehaviour
{
    public SceneFader fader;
    public string playerTag = "Player";
    public float timeBeforeFade = 1.0f; // Temps pour voir l'anim de mort

    private bool isRespawning = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(playerTag) && !isRespawning)
        {
            isRespawning = true;
            PlayerMovement pm = collision.GetComponent<PlayerMovement>();
            
            if (pm != null)
            {
                StartCoroutine(HandleRespawnSequence(pm));
            }
        }
    }

    private IEnumerator HandleRespawnSequence(PlayerMovement pm)
    {
        // 1. Immobilisation et animation de mort
        pm.TriggerDeath();

        // 2. On attend un peu pour que le joueur voit son perso mourir
        yield return new WaitForSeconds(timeBeforeFade);

        // 3. On lance le fondu et la téléportation
        yield return StartCoroutine(fader.FadeOutAndTeleport(pm.transform, pm.GetRespawnPosition()));
        
        // 4. On réactive le joueur (mouvement + anim idle)
        pm.ResetAfterRespawn();
        
        isRespawning = false;
    }
}