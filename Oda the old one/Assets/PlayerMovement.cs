using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Composants")]
    private Rigidbody2D rb;
    private Animator anim;
    private TrailRenderer trail;

    [Header("Mouvement")]
    public float walkSpeed = 7f; 
    public float jumpForce = 14f;
    private float horizontalInput;

    [Header("Saut Avancé (Celeste-like)")]
    public float fallMultiplier = 3f;
    public float lowJumpMultiplier = 2.5f;
    public float coyoteTime = 0.15f;
    private float coyoteTimeCounter;
    public float jumpBufferTime = 0.15f;
    private float jumpBufferCounter;
    private bool isJumpPressed;

    [Header("Wall Jump")]
    public Transform wallCheck; 
    public float wallCheckRadius = 0.25f;
    public float wallSlidingSpeed = 2f; 
    public Vector2 wallJumpForce = new Vector2(12f, 16f); 
    public float wallJumpDuration = 0.25f; 
    private bool isWallSliding;
    private bool isWallJumping;

    [Header("Dash")]
    public float dashForce = 20f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;
    private bool canDash = true;
    private bool isDashing;

    [Header("Animations & État")]
    public bool isAttacking;
    public bool isDead;

    [Header("Sol & Effets")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public GameObject dustPrefab;
    private bool isGrounded;
    private bool wasGrounded;

    [Header("Système de Respawn & Save")]
    private Vector2 startPosition; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        trail = GetComponent<TrailRenderer>(); 
        
        // --- CHARGEMENT DE LA SAUVEGARDE ---
        int currentSlot = PlayerPrefs.GetInt("CurrentSlot", 0);
        PlayerData data = SaveSystem.Load(currentSlot);

        if (data != null)
        {
            Vector2 savedPos = new Vector2(data.x, data.y);
            
            // On force la position sur le Transform ET le Rigidbody
            transform.position = savedPos;
            if (rb != null) rb.linearVelocity = Vector2.zero; // Stop tout mouvement résiduel
            
            startPosition = savedPos;
            Debug.Log("<color=green>Position chargée avec succès du Slot " + currentSlot + " : " + savedPos + "</color>");
        }
        else
        {
            startPosition = transform.position;
            Debug.Log("Aucune sauvegarde trouvée, position par défaut.");
        }

        if (trail != null) trail.emitting = false;
    }

    void Update()
    {
        if (isDead || isDashing) return;

        CheckSurroundings();
        HandleWallSliding();
        HandleBuffers();
        UpdateAnimations();
        
        if (!isWallJumping && !isWallSliding) Flip();

        if (jumpBufferCounter > 0f)
        {
            if (coyoteTimeCounter > 0f && !isAttacking) PerformJump();
            else if (isWallSliding) StartCoroutine(PerformWallJump());
        }
    }

    void FixedUpdate()
    {
        if (isDead || isDashing || isWallJumping) return;

        if (!isAttacking)
        {
            rb.linearVelocity = new Vector2(horizontalInput * walkSpeed, rb.linearVelocity.y);
        }

        ApplyBetterJumpPhysics();
    }

    private void CheckSurroundings()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        bool isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, groundLayer);
        isWallSliding = isTouchingWall && !isGrounded && rb.linearVelocity.y < 0;

        if (isGrounded)
        {
            canDash = true; 
            if (!wasGrounded) CreateDust();
        }
        wasGrounded = isGrounded;
    }

    private void HandleWallSliding()
    {
        if (isWallSliding)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -wallSlidingSpeed, float.MaxValue));
        }
    }

    private void HandleBuffers()
    {
        if (isGrounded) coyoteTimeCounter = coyoteTime;
        else coyoteTimeCounter -= Time.deltaTime;

        jumpBufferCounter -= Time.deltaTime;
    }

    private void ApplyBetterJumpPhysics()
    {
        if (rb.linearVelocity.y < 0)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        else if (rb.linearVelocity.y > 0 && !isJumpPressed)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
    }

    private void PerformJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpBufferCounter = 0f;
        coyoteTimeCounter = 0f;
        CreateDust();
    }

    private IEnumerator PerformWallJump()
    {
        isWallJumping = true;
        jumpBufferCounter = 0f;
        coyoteTimeCounter = 0f;

        float jumpDir = transform.localScale.x; 
        transform.localScale = new Vector3(-transform.localScale.x, 1, 1);
        rb.linearVelocity = new Vector2(jumpDir * wallJumpForce.x, wallJumpForce.y);

        yield return new WaitForSeconds(wallJumpDuration);
        isWallJumping = false;
    }

    private IEnumerator PerformDash()
    {
        canDash = false; 
        isDashing = true;
        if (trail != null) trail.emitting = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        float dashDirection = -transform.localScale.x; 
        rb.linearVelocity = new Vector2(dashDirection * dashForce, 0f);

        yield return new WaitForSeconds(dashDuration);

        if (trail != null) trail.emitting = false;
        rb.gravityScale = originalGravity;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.5f, 0f);
        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
    }

    // --- INTERFACE POUR LE FALL DETECTOR ET CHECKPOINTS ---

    public void UpdateCheckpoint(Vector2 newPos)
    {
        startPosition = newPos;
    }

    public Vector2 GetRespawnPosition()
    {
        return startPosition;
    }

    public void TriggerDeath()
    {
        if (isDead) return;
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
        anim.SetBool("isDead", true);
    }

    public void ResetAfterRespawn()
    {
        isDead = false;
        rb.bodyType = RigidbodyType2D.Dynamic;
        anim.SetBool("isDead", false);
        anim.Play("Iddle");
    }

    // --- INPUTS & VISUEL ---

    public void OnMove(InputValue value) => horizontalInput = value.Get<Vector2>().x;
    public void OnJump(InputValue value) {
        if (value.isPressed) { jumpBufferCounter = jumpBufferTime; isJumpPressed = true; }
        else isJumpPressed = false;
    }
    public void OnDash(InputValue value) { if (value.isPressed && canDash && !isDead) StartCoroutine(PerformDash()); }

    private void UpdateAnimations() {
        if (isDead) return;
        anim.SetBool("isWalking", Mathf.Abs(horizontalInput) > 0.1f && !isAttacking);
    }

    private void Flip() {
        if (isAttacking || isDashing || isDead || horizontalInput == 0) return; 
        if (horizontalInput > 0) transform.localScale = new Vector3(-1, 1, 1);
        else if (horizontalInput < 0) transform.localScale = new Vector3(1, 1, 1);
    }

    private void CreateDust() { if (dustPrefab != null) Instantiate(dustPrefab, groundCheck.position, Quaternion.identity); }
}