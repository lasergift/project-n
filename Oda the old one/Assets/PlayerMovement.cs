using UnityEngine;
using UnityEngine.InputSystem; // Requis pour le nouveau système d'Input

public class PlayerMovement : MonoBehaviour
{
    [Header("Réglages Mouvement")]
    public float walkSpeed = 8f;
    public float runSpeed = 14f;
    public float jumpForce = 12f;
    [Tooltip("Multiplicateur de gravité lors de la chute")]
    public float fallMultiplier = 4f; 

    [Header("Détection Sol")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    private Rigidbody2D rb;
    private Animator anim;
    private float horizontalInput;
    private bool isGrounded;
    private bool isRunning;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    // Détecte le mouvement (touches fléchées / WASD)
    public void OnMove(InputValue value)
    {
        horizontalInput = value.Get<Vector2>().x;
    }

    // Détecte l'appui ET le relâchement de Shift
    public void OnSprint(InputValue value)
    {
        // value.isPressed devient faux automatiquement quand on relâche la touche
        isRunning = value.isPressed;
    }

    // Détecte le saut
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
        // On choisit la vitesse en fonction de l'état de course
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        
        // Appliquer le mouvement horizontal
        rb.linearVelocity = new Vector2(horizontalInput * currentSpeed, rb.linearVelocity.y);

        // --- PHYSIQUE DE CHUTE ---
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    private void UpdateAnimations()
    {
        // On marche si on bouge
        anim.SetBool("isWalking", horizontalInput != 0);
        
        // On court SEULEMENT si Shift est pressé ET qu'on bouge
        // Cela règle le problème du perso qui court sur place ou redémarre en run
        anim.SetBool("isRunning", isRunning && horizontalInput != 0);
        
        // État du sol
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

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}