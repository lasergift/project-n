using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Composants")]
    private Rigidbody2D rb;
    private Animator anim;
    private TrailRenderer trail; // Ajouté pour le rendu de traînée

    [Header("Mouvement")]
    public float walkSpeed = 7f; 
    public float jumpForce = 14f;
    private float horizontalInput;

    [Header("Dash (Celeste-like)")]
    public float dashForce = 20f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;
    private bool canDash = true;
    private bool isDashing;

    [Header("Sol & Effets")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public GameObject dustPrefab;
    private bool isGrounded;
    private bool wasGrounded;

    [Header("Combat & État")]
    public bool isAttacking;
    public bool isDead;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        trail = GetComponent<TrailRenderer>(); // Récupération automatique du Trail Renderer
        
        if (trail != null) trail.emitting = false; // Désactivé par défaut
    }

    void Update()
    {
        if (isDead || isDashing) return;

        CheckGround();
        UpdateAnimations();
        Flip();
    }

    void FixedUpdate()
    {
        if (isDead || isDashing) return;

        if (!isAttacking)
        {
            rb.linearVelocity = new Vector2(horizontalInput * walkSpeed, rb.linearVelocity.y);
        }
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            canDash = true; 
            if (!wasGrounded) CreateDust();
        }
        wasGrounded = isGrounded;
    }

    // --- INPUT SYSTEM ---
    public void OnMove(InputValue value) => horizontalInput = value.Get<Vector2>().x;

    public void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded && !isAttacking && !isDead)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            CreateDust();
        }
    }

    public void OnDash(InputValue value)
    {
        if (value.isPressed && canDash && !isDead)
        {
            StartCoroutine(PerformDash());
        }
    }

    public void OnAttack(InputValue value)
    {
        if (value.isPressed && isGrounded && !isAttacking && !isDead)
        {
            StartCoroutine(PerformAttack());
        }
    }

    // --- ACTIONS ---

    private IEnumerator PerformDash()
    {
        canDash = false;
        isDashing = true;
        
        if (trail != null) trail.emitting = true; // Allume la traînée

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        // Direction inversée pour tes sprites qui regardent à gauche par défaut
        float dashDirection = -transform.localScale.x; 
        rb.linearVelocity = new Vector2(dashDirection * dashForce, 0f);

        yield return new WaitForSeconds(dashDuration);

        if (trail != null) trail.emitting = false; // Éteint la traînée
        
        rb.gravityScale = originalGravity;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.5f, 0f);
        isDashing = false;
        
        yield return new WaitForSeconds(dashCooldown);
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        anim.SetTrigger("attack");
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.4f); 
        isAttacking = false;
    }

    private void UpdateAnimations()
    {
        bool walking = Mathf.Abs(horizontalInput) > 0.1f && !isAttacking;
        anim.SetBool("isWalking", walking);
    }

    private void CreateDust()
    {
        if (dustPrefab != null) {
            Instantiate(dustPrefab, groundCheck.position, Quaternion.identity);
        }
    }

    private void Flip()
    {
        if (isAttacking || isDashing || horizontalInput == 0) return; 

        if (horizontalInput > 0) transform.localScale = new Vector3(-1, 1, 1);
        else if (horizontalInput < 0) transform.localScale = new Vector3(1, 1, 1);
    }
}