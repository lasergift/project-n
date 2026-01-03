using UnityEngine;
using UnityEngine.InputSystem; // Requis pour le nouveau système d'Input

public class PlayerMovement : MonoBehaviour
{
    [Header("Réglages Mouvement")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    [Tooltip("Multiplicateur de gravité lors de la chute pour éviter l'effet 'lune'")]
    public float fallMultiplier = 4f; 

    [Header("Détection Sol")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    private Rigidbody2D rb;
    private Animator anim;
    private float horizontalInput;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    // Appelée par le composant Player Input (Action 'Move')
    public void OnMove(InputValue value)
    {
        horizontalInput = value.Get<Vector2>().x;
    }

    // Appelée par le composant Player Input (Action 'Jump')
    public void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    void Update()
    {
        // 1. Détection du sol
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // 2. Mise à jour de l'Animator
        UpdateAnimations();

        // 3. Gestion du Flip (Miroir)
        FlipCharacter();
    }

    void FixedUpdate()
    {
        // Appliquer le mouvement horizontal
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        // --- PHYSIQUE DE CHUTE (Gravité augmentée) ---
        if (rb.linearVelocity.y < 0)
        {
            // On ajoute une force vers le bas quand le joueur tombe
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    private void UpdateAnimations()
    {
        // Variable pour marcher
        anim.SetBool("isWalking", horizontalInput != 0);
        
        // Variable pour le saut/chute
        anim.SetBool("isGrounded", isGrounded);
    }

    private void FlipCharacter()
    {
        if (horizontalInput > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (horizontalInput < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    // Visualisation de la zone de détection du sol dans l'éditeur (Gizmos)
    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}