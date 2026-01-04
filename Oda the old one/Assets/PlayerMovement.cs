using UnityEngine;
using UnityEngine.InputSystem; // Required for the new Input System

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 8f;
    public float runSpeed = 14f;

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

    void FixedUpdate()
    {
        // Choose speed based on sprint state
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

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
