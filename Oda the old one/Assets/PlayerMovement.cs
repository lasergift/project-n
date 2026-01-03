using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Réglages Mouvement")]
    public float walkSpeed = 8f;
    public float runSpeed = 14f; // Vitesse quand on court
    public float jumpForce = 12f;
    public float fallMultiplier = 4f;

    [Header("Détection Sol")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    private Rigidbody2D rb;
    private Animator anim;
    private float horizontalInput;
    private bool isGrounded;
    private bool isRunning; // Pour savoir si Shift est pressé

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    public void OnMove(InputValue value)
    {
        horizontalInput = value.Get<Vector2>().x;
    }

    // Cette fonction détecte l'appui sur Shift (Action 'Sprint' ou 'Run' dans l'Input System)
    // Assure-toi d'avoir une action nommée "Sprint" liée à la touche Shift dans ton Input Action Asset
    public void OnSprint(InputValue value)
    {
        isRunning = value.isPressed;
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Mise à jour de l'Animator
        anim.SetBool("isWalking", horizontalInput != 0);
        anim.SetBool("isRunning", isRunning && horizontalInput != 0); // On court seulement si on bouge
        anim.SetBool("isGrounded", isGrounded);

        FlipCharacter();
    }

    void FixedUpdate()
    {
        // On choisit la vitesse actuelle
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        rb.linearVelocity = new Vector2(horizontalInput * currentSpeed, rb.linearVelocity.y);

        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    private void FlipCharacter()
    {
        if (horizontalInput > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (horizontalInput < 0) transform.localScale = new Vector3(-1, 1, 1);
    }
}