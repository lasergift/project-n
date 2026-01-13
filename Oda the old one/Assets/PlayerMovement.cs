using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Composants")]
    private Rigidbody2D rb;
    private Animator anim;

    [Header("Mouvement")]
    public float walkSpeed = 7f; 
    public float jumpForce = 14f;
    private float horizontalInput;

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
    }

    void Update()
    {
        if (isDead) return;

        CheckGround();
        UpdateAnimations();
        Flip();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        if (!isAttacking)
        {
            rb.linearVelocity = new Vector2(horizontalInput * walkSpeed, rb.linearVelocity.y);
        }
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded && !wasGrounded) {
            CreateDust();
        }
        wasGrounded = isGrounded;
    }

    public void OnMove(InputValue value)
    {
        horizontalInput = value.Get<Vector2>().x;
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded && !isAttacking && !isDead)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            CreateDust();
        }
    }

    public void OnAttack(InputValue value)
    {
        if (value.isPressed && isGrounded && !isAttacking && !isDead)
        {
            StartCoroutine(PerformAttack());
        }
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        anim.SetTrigger("attack");
        rb.linearVelocity = Vector2.zero;
        
        yield return new WaitForSeconds(0.4f); 

        isAttacking = false;
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        anim.SetBool("isDead", true);
        rb.linearVelocity = Vector2.zero;
    }

    private void UpdateAnimations()
    {
        bool walking = Mathf.Abs(horizontalInput) > 0.1f && !isAttacking;
        anim.SetBool("isWalking", walking);
    }

    private void Flip()
    {
        if (isAttacking || horizontalInput == 0) return; 

        // Inversion du sens horizontal :
        // Si tes sprites regardent à GAUCHE par défaut, on met -1 pour aller à DROITE
        if (horizontalInput > 0) 
            transform.localScale = new Vector3(-1, 1, 1);
        else if (horizontalInput < 0) 
            transform.localScale = new Vector3(1, 1, 1);
    }

    private void CreateDust()
    {
        if (dustPrefab != null) {
            Instantiate(dustPrefab, groundCheck.position, Quaternion.identity);
        }
    }
}