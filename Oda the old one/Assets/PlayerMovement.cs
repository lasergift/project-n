using UnityEngine;
using UnityEngine.InputSystem; // Required for the new Input System
using System.Collections;
public class PlayerMovement : MonoBehaviour
{
[Header("Combat Combo")]
public bool isAttacking = false;
private int comboStep = 0; // 0 = Idle, 1 = Attaque 1, etc.
private float lastClickTime;
public float comboDelay = 0.5f; // Temps max entre deux clics pour continuer le combo

public void OnAttack(InputValue value)
{
    if (value.isPressed && isGrounded && !isAttacking)
    {
        // On calcule le temps depuis la dernière attaque
        float timeSinceLastClick = Time.time - lastClickTime;

        if (timeSinceLastClick <= comboDelay)
        {
            comboStep++;
            if (comboStep > 3) comboStep = 1; // Recommence après la 3ème
        }
        else
        {
            comboStep = 1; // Trop lent, on recommence à 1
        }

        lastClickTime = Time.time;
        StartCoroutine(PerformAttack());
    }
}

private IEnumerator PerformAttack()
{
    isAttacking = true;

    // On envoie les infos à l'animator
    anim.SetInteger("attackIndex", comboStep);
    anim.SetTrigger("attack");
    // Ajoute ça juste après anim.SetTrigger
    float dashForce = 3f;
    rb.AddForce(new Vector2(transform.localScale.x * dashForce, 0), ForceMode2D.Impulse);
    // On stop le mouvement pendant l'animation
    Vector2 originalVelocity = rb.linearVelocity;
    rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

    // Temps d'attente (ajuste selon la vitesse de tes animations)
    // Plus ce temps est court, plus le combo est nerveux
    yield return new WaitForSeconds(0.3f); 

    isAttacking = false;
}
    [Header("Movement Settings")]
    public float walkSpeed = 8f;
    public float runSpeed = 14f;
    public float transitionDamp = 3f;

    [Header("Jump Settings (Height & Time Based)")]
    [Tooltip("Maximum height reached by the jump")]
    public float jumpHeight = 3f;

    [Tooltip("Time (in seconds) to reach the top of the jump")]
    public float timeToApex = 0.4f;

    [Tooltip("Extra gravity multiplier when falling")]
    public float fallMultiplier = 4f;

    [Header("Ground Detection")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    private Rigidbody2D rb;
    private Animator anim;
    private float horizontalInput;
    private bool isGrounded;
    private bool isRunning;

    // Computed jump physics values
    private float jumpVelocity;
    private float gravityStrength;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        rb.gravityScale = 0f;

        // Compute gravity and initial jump velocity from height & time
        gravityStrength = -(2f * jumpHeight) / (timeToApex * timeToApex);
        jumpVelocity = Mathf.Abs(gravityStrength) * timeToApex;

        // Apply custom gravity to the Rigidbody
        rb.gravityScale = gravityStrength / Physics2D.gravity.y;
    }

    // Detect horizontal movement (Arrow keys / WASD)
    public void OnMove(InputValue value)
    {
        horizontalInput = value.Get<Vector2>().x;
    }

    // Detect sprint press and release (Shift)
    public void OnSprint(InputValue value)
    {
        isRunning = value.isPressed;
    }

    // Detect jump input
    public void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);
            isGrounded = false;
        }
    }

    void Update()
    {
        // 1. Ground detection
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        // 2. Update animations
        UpdateAnimations();

        // 3. Handle character flip
        FlipCharacter();
    }
    float currentSpeed = 0f;
    void FixedUpdate()
    {
        // Choose speed based on sprint state
        float targetSpeed = isRunning ? runSpeed : walkSpeed;
        
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, transitionDamp * Time.deltaTime);

        // Apply horizontal movement only when grounded
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(
                horizontalInput * currentSpeed,
                rb.linearVelocity.y
            );
        }
        // Apply custom gravity based on jump timing
        float gravityThisFrame = gravityStrength;

        // Increase gravity when falling for snappier descent
        if (rb.linearVelocity.y < 0)
        {
            gravityThisFrame *= fallMultiplier * Time.fixedDeltaTime;
        }

        // Apply gravity manually
        rb.linearVelocity += Vector2.up *
                             gravityThisFrame *
                             Time.fixedDeltaTime;

    }

    private void UpdateAnimations()
    {
        // Walking state
        anim.SetBool("isWalking", horizontalInput != 0);

        // Running only if sprinting AND moving
        anim.SetBool("isRunning", isRunning && horizontalInput != 0);

        // Grounded state
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
