using UnityEngine;
using System.Collections;

public class BlinkSprite : MonoBehaviour
{
    [Header("Réglages du Clignotement")]
    public float blinkSpeed = 2f;      // Vitesse de pulsation
    public float minScale = 0.8f;     // Taille mini
    public float maxScale = 1.2f;     // Taille maxi
    
    [Header("Options")]
    public bool useScale = true;      // Faire varier la taille
    public bool useAlpha = true;      // Faire varier la transparence

    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private bool isBlinking = true;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;

        if (spriteRenderer == null)
        {
            Debug.LogError("BlinkSprite: Aucun SpriteRenderer trouvé sur " + gameObject.name);
            return;
        }

        StartCoroutine(DoBlink());
    }

    IEnumerator DoBlink()
    {
        while (isBlinking)
        {
            // Calcul d'une valeur qui ondule entre 0 et 1 (Sinus)
            float lerpValue = (Mathf.Sin(Time.time * blinkSpeed * Mathf.PI) + 1f) * 0.5f;

            // Effet de Taille (Scale)
            if (useScale)
            {
                float s = Mathf.Lerp(minScale, maxScale, lerpValue);
                transform.localScale = originalScale * s;
            }

            // Effet de Transparence (Alpha)
            if (useAlpha)
            {
                Color c = spriteRenderer.color;
                c.a = Mathf.Lerp(0.3f, 1f, lerpValue); // Varie entre 30% et 100% d'opacité
                spriteRenderer.color = c;
            }

            yield return null;
        }
    }
    
    // Pour arrêter l'effet quand le joueur a compris (via un autre script)
    public void StopBlinking()
    {
        isBlinking = false;
        transform.localScale = originalScale;
        Color c = spriteRenderer.color;
        c.a = 1f;
        spriteRenderer.color = c;
    }
}