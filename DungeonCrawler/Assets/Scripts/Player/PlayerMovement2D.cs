using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(PlayerAttributes))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Player Attributes")]
    [SerializeField] private float movementSpeed = 1;
    [SerializeField] private float deaccelerationMultiplier = 0.15f;
    [Range(0.1f, 0.9f)] [SerializeField] private float crouchMultiplier = 0.5f;
    [SerializeField] private float jumpVelocity = 1;
    [SerializeField] private float jumpMultiplier = 1;
    [SerializeField] private float fallMultiplier = 2.5f;

    [Header("Debug")]
    [SerializeField] private bool crouching = false;
    [SerializeField] private bool grounded = false;
    [SerializeField] private bool jumping = false;
    [SerializeField] public bool transitioning = false;

    private Rigidbody2D playerRb2D; 
    private Vector3 originalScale;

    void Start()
    {
        playerRb2D = this.GetComponent<Rigidbody2D>();
        originalScale = this.transform.localScale;
        this.gameObject.tag = "Player";
    }

    void Update()
    {
        if (transitioning)
            return;

        checkJumpInput();
    }

    void FixedUpdate()
    {
        checkGrounded();

        if (transitioning)
            return;

        groundPlayer();
        checkPlayerInputH();
        playerVelocityCancel();
        jump();
        crouch();
    }

    Vector2 checkPlayerInputH()
    {
        var horizontalInput = Input.GetAxisRaw("Horizontal") * Time.deltaTime * movementSpeed;

        playerRb2D.velocity = new Vector2(
            Mathf.Clamp(playerRb2D.velocity.x + horizontalInput,-movementSpeed * Time.deltaTime, movementSpeed * Time.deltaTime),
            playerRb2D.velocity.y);

        if (crouching)
            playerRb2D.velocity = new Vector2(playerRb2D.velocity.x * crouchMultiplier, playerRb2D.velocity.y);

        return Vector2.right * horizontalInput;
    }

    void playerVelocityCancel()
    {
        if (playerRb2D.velocity.x < -0.1f)
            playerRb2D.velocity += Vector2.right * movementSpeed * deaccelerationMultiplier * Time.deltaTime ;
        else if (playerRb2D.velocity.x > 0.1f)
            playerRb2D.velocity -= Vector2.right * movementSpeed * deaccelerationMultiplier * Time.deltaTime ;
        else if (-0.1f < playerRb2D.velocity.x  && playerRb2D.velocity.x < 0.1f)
            playerRb2D.velocity = new Vector2(0,playerRb2D.velocity.y);
    }

    bool crouch()
    {
        var crouchInput = Input.GetAxisRaw("VerticalNeg");
        crouching = crouchInput != 0;

        this.transform.localScale = crouching == true ? originalScale / 2 : originalScale;
        this.transform.DOScale(crouching == true ? originalScale / 2 : originalScale, 0.3f);

        return crouching;
    }
    
    public bool getCrouching() => crouching;

    bool checkJumpInput()
    {
        if (Input.GetButtonDown("VerticalPos") && grounded)
            jumping = true;

        return jumping;
    }

    void jump()
    {
        if (jumping)
        {
            //playerRb2D.velocity += Vector2.up * jumpVelocity * Time.deltaTime;

            playerRb2D.AddForce(Vector2.up * jumpVelocity * Time.deltaTime, ForceMode2D.Impulse);
            jumping = false;
        }
    }

    bool checkGrounded()
    {
        int layer = 1 << 9;
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position, Vector2.down, Mathf.Infinity, layer);
        //grounded = Vector2.Distance(hit.point, this.transform.position) <= (this.GetComponent<Collider2D>().bounds.size.y * 0.6f);
        var distance = (hit.point - this.GetComponent<Rigidbody2D>().position).magnitude;
        grounded = distance > 0 && distance <= (this.GetComponent<Collider2D>().bounds.size.y * 0.55f);

        return grounded;
    }

    public bool getGrounded() => grounded;

    void groundPlayer()
    {
       if (playerRb2D.velocity.y < 0)
           playerRb2D.velocity += Vector2.up * Physics2D.gravity * fallMultiplier * Time.deltaTime;
       else if (playerRb2D.velocity.y > 0 && !Input.GetButton("VerticalPos"))
           playerRb2D.velocity += Vector2.up * Physics2D.gravity * jumpMultiplier * Time.deltaTime;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawRay(this.transform.position, Vector3.down);
    }
}
