using UnityEngine;
using UnityEngine.InputSystem; // Obligatoire pour le nouveau système

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    public LayerMask groundLayer;
    public Transform groundCheck;

    private Rigidbody2D rb;
    private Animator anim;
    private float horizontalInput;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    // Cette fonction est appelée automatiquement par le composant Player Input
    public void OnMove(InputValue value)
    {
        // Récupère la direction (gauche/droite)
        horizontalInput = value.Get<Vector2>().x;
    }

    // Cette fonction est appelée quand on appuie sur le bouton de saut (Espace)
    public void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    void Update()
    {
        // Logique de détection du sol
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        // Mise à jour de l'animation
        bool walking = horizontalInput != 0;
        anim.SetBool("isWalking", walking);

        // Logique du Flip (Miroir)
        if (horizontalInput > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (horizontalInput < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    void FixedUpdate()
    {
        // Application du mouvement physique
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }
}