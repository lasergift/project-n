using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public Image fadeImage;
    public float fadeSpeed = 1.5f;

void Start()
{
    // On force l'image à être transparente au démarrage
    fadeImage.color = new Color(0, 0, 0, 0);

    // Supprime ou commente la ligne ci-dessous si tu ne veux pas de fondu au lancement
    // StartCoroutine(FadeIn()); 
}

    public IEnumerator FadeIn()
    {
        float alpha = 1f;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
    }

    public IEnumerator FadeOutAndTeleport(Transform player, Vector3 spawnPosition)
    {
        // 1. Fondu vers le noir
        float alpha = 0f;
        while (alpha < 1)
        {
            alpha += Time.deltaTime * fadeSpeed;
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // 2. Téléportation pendant que l'écran est noir
        player.position = spawnPosition;
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // 3. Petit temps d'arrêt pour le confort
        yield return new WaitForSeconds(0.2f);

        // 4. Retour au transparent
        StartCoroutine(FadeIn());
    }
}