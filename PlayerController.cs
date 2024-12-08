using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //ESSENTIALS
    private Rigidbody2D rb;
    private Collider2D col;

    [Header("Movement")]

    [Header("Horizontal movement")] //acceleration - max speed - deceleration - quick turn
    public float direction;
    public float acceleration;
    public float deceleration;
    public float maxSpeed;
    public float currentSpeed;

    [Header("Vertical movement")] //coyote time - jump - bonus air time - fast fall
    public float jumpForce;
    public float maxFallSpeed;
    public float initialGravity;
    public float jumpCutGravity;
    public float fallGravity;
    public bool isJumping;
    public bool isFalling;
    public float hangTimeVelocityThreshold;
    public float hangTimeGravity;
    public bool isGrounded;
    public float groundCheckDistance;
    public LayerMask groundLayer;

    [Header("Bonuses")]
    public float coyoteTime;
    public float coyoteTimeCounter;
    public float jumpBufferTime;
    public float jumpBufferCounter;

    void Awake()
    {
        currentSpeed = 0;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //MOVEMENT

        //HORIZONTAL MOVEMENT
        if (Input.GetKey(KeyCode.RightArrow)) //right movement
        {
            direction = 1;
            Movement();
        }
        else if (Input.GetKey(KeyCode.LeftArrow)) //left movement
        {
            direction = -1;
            Movement();
        }
        else
        {
            Decelerate(); //slow down
        }

        //VERTICAL MOVEMENT
        RaycastHit2D groundCheck = Physics2D.Raycast(new Vector2(col.bounds.min.x + 0.05f, col.bounds.min.y - groundCheckDistance), Vector2.right, col.bounds.max.x - col.bounds.min.x - 0.05f, groundLayer); //checking in a horizontal line right bellow player's feet
        isGrounded = groundCheck.collider != null;
        Debug.DrawLine(new Vector2(col.bounds.min.x, col.bounds.min.y - groundCheckDistance), new Vector2(col.bounds.max.x, col.bounds.min.y - groundCheckDistance), Color.red);

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime; //can jump for a short period after already falling off a ledge
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime; //can jump even if the button was pressed slightly before hitting the ground
        }

        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0 && isJumping == false) //jump
        {
            rb.linearVelocityY = jumpForce;
            isFalling = false;
            isJumping = true;
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            coyoteTimeCounter = 0;
        }

        if (isJumping && Mathf.Abs(rb.linearVelocityY) <= hangTimeVelocityThreshold)
        {
            rb.gravityScale = hangTimeGravity; //bonus air time
            isJumping = false;
        }

        if (rb.linearVelocityY < -0.01) //fall
        {
            rb.gravityScale = fallGravity; //fast fall
            rb.linearVelocityY = Mathf.Clamp(rb.linearVelocityY, -maxFallSpeed, 0); //can't fall faster than max
            isFalling = true;
        }
        else if (!Input.GetKey(KeyCode.Space) && isJumping)
        {
            rb.gravityScale = jumpCutGravity; //cuts the jump when the button is released
        }
        else if (!isJumping)
        {
            isFalling = false;
            rb.gravityScale = initialGravity;
        }
    }

    void Movement()
    {
        currentSpeed += acceleration * Time.deltaTime;        
        currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeed); //can't go above max

        if (rb.linearVelocityX * direction < 0)
        {
            currentSpeed = maxSpeed; //quick turn
        }

        rb.linearVelocityX = currentSpeed * direction;
    }

    void Decelerate()
    {
        currentSpeed -= deceleration * Time.deltaTime;

        if (rb.linearVelocityX == 0 || (rb.linearVelocityX <= 0 && direction > 0) || (rb.linearVelocityX >= 0 && direction < 0))
        {
            currentSpeed = 0; //makes you stop from going in the opposite direction
        }

        rb.linearVelocityX = currentSpeed * direction;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("death"))
        {
            transform.position = new Vector2(-12.69f, 6.56f);
        }
    }
}
