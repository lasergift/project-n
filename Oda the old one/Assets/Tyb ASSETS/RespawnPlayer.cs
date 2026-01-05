using UnityEngine;
using System.Collections; // <-- CETTE LIGNE MANQUAIT

public class RespawnPlayer : MonoBehaviour
{
    public Transform spawnPoint;
    public SceneFader fader; // Glisse le FadeCanvas ici
    public string playerTag = "Player";

    private bool isRespawning = false; // Empêche de lancer le fondu plusieurs fois

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // On vérifie le tag et on s'assure qu'on n'est pas déjà en train de respawn
        if (collision.CompareTag(playerTag) && !isRespawning)
        {
            isRespawning = true;
            StartCoroutine(HandleRespawn(collision.transform));
        }
    }

    private IEnumerator HandleRespawn(Transform playerTransform)
    {
        // On lance le fondu et on attend qu'il finisse
        // StartCoroutine retourne une instruction d'attente
        yield return StartCoroutine(fader.FadeOutAndTeleport(playerTransform, spawnPoint.position));
        
        isRespawning = false;
    }
}