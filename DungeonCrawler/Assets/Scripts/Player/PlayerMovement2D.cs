using DG.Tweening;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(PlayerAttributes))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Movements Values")]
    [SerializeField] private float movementSpeed = 1;
    [SerializeField] private float deaccelerationMultiplier = 0.15f;
    [Range(0.1f, 0.9f)] [SerializeField] private float crouchMultiplier = 0.5f;
    [SerializeField] private float fallMultiplier = 2.5f;

    [Header("Jump Values")]
    [SerializeField] private int maxJumps = 1;
    [SerializeField] private float jumpVelocity = 1;
    [SerializeField] private float jumpMultiplier = 1;

    private int currentJumps;

    [Header("Dash Values")]
    [SerializeField] private float dashDistance = 1f;
    [SerializeField] private float dashSpeed = 0.5f;
    [SerializeField] private bool iFrames = true;

    [Header("Double Tap Input values")]
    [SerializeField] private float tapSpeed = 0.5f;

    private float lastTapTimeA = 0;
    private float lastTapTimeD = 0;

    [Header("Debug")]
    [SerializeField] private bool crouching = false;
    [SerializeField] private bool grounded = false;
    [SerializeField] private bool jumping = false;
    [SerializeField] private bool dashing = false;
    [SerializeField] public bool transitioning = false;

    private Rigidbody2D playerRb2D;
    private Collider2D playerCollider;
    private Vector3 originalScale;

    void Start()
    {
        playerRb2D = this.GetComponent<Rigidbody2D>();
        playerCollider = this.GetComponent<Collider2D>();
        originalScale = this.transform.localScale;
        this.gameObject.tag = "Player";

        lastTapTimeA = lastTapTimeD = 0;
    }

    void Update()
    {
        if (transitioning)
            return;

        checkJumpInput();
        dash();
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

    private void LateUpdate()
    {
        if (currentJumps < maxJumps && grounded)
            currentJumps = maxJumps;

    }

    Vector2 checkPlayerInputH()
    {
        var horizontalInput = Input.GetAxisRaw("Horizontal") * Time.deltaTime * movementSpeed;

        playerRb2D.velocity = new Vector2(
            math.clamp(playerRb2D.velocity.x + horizontalInput, -movementSpeed * Time.deltaTime, movementSpeed * Time.deltaTime),
            playerRb2D.velocity.y);

        if (crouching)
            playerRb2D.velocity = new Vector2(playerRb2D.velocity.x * crouchMultiplier, playerRb2D.velocity.y);

        return Vector2.right * horizontalInput;
    }

    void playerVelocityCancel()
    {
        if (playerRb2D.velocity.x < -0.1f)
            playerRb2D.velocity += Vector2.right * movementSpeed * deaccelerationMultiplier * Time.deltaTime;
        else if (playerRb2D.velocity.x > 0.1f)
            playerRb2D.velocity -= Vector2.right * movementSpeed * deaccelerationMultiplier * Time.deltaTime;
        else if (-0.1f < playerRb2D.velocity.x && playerRb2D.velocity.x < 0.1f)
            playerRb2D.velocity = new Vector2(0, playerRb2D.velocity.y);
    }

    void dash()
    {
        if (dashing)
            return;

        if (Input.GetKeyDown(KeyCode.A))
        {
            if ((Time.time - lastTapTimeA) < tapSpeed)
                dashing = true;

            lastTapTimeA = Time.time;
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            if ((Time.time - lastTapTimeD) < tapSpeed)
                dashing = true;

            lastTapTimeD = Time.time;
        }

        if (dashing)
        {
            var input = Input.GetAxisRaw("Horizontal");
            if (input == 0)
            {
                dashing = false;
                return;
            }

            StartCoroutine(DashPlayer(new Vector3(input, 0, 0) * checkDashCollision(input)));

            Camera.main.DOComplete();
            Camera.main.DOShakePosition(0.2f, 0.1f);
        }
    }

    IEnumerator DashPlayer(Vector3 direction)
    {
        if (iFrames)
            playerCollider.isTrigger = true;
        playerRb2D.velocity = Vector2.zero;
        playerRb2D.gravityScale = 0;

        var tween = playerRb2D.DOMove(this.transform.position + direction, dashSpeed * math.abs(direction.x / dashDistance));
        yield return tween.WaitForCompletion();

        if (iFrames)
            playerCollider.isTrigger = false;
        dashing = false;
        playerRb2D.gravityScale = 1;
    }

    float checkDashCollision(float direction)
    {
        int layer = 1 << 9;
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position, new Vector2(direction, 0), math.INFINITY, layer);
        var distance = (hit.point - playerRb2D.position).magnitude;

        if (distance < dashDistance)
            return distance - 0.5f;

        return dashDistance;
    }

    bool crouch()
    {
        var crouchInput = Input.GetAxisRaw("VerticalNeg");
        crouching = crouchInput != 0;

        //this.transform.localScale = crouching ? originalScale / 2 : originalScale;
        this.transform.DOScale(crouching ? originalScale / 2 : originalScale, 0.3f);

        return crouching;
    }

    public bool getCrouching() => crouching;

    bool checkJumpInput()
    {
        if (Input.GetButtonDown("VerticalPos") && currentJumps > 0)
            jumping = true;

        return jumping;
    }

    void jump()
    {
        if (currentJumps < 1) return;

        if (jumping)
        {
            playerRb2D.AddForce(Vector2.up * jumpVelocity * Time.deltaTime, ForceMode2D.Impulse);
            jumping = false;

            currentJumps--;
        }
    }

    bool checkGrounded()
    {
        int layer = 1 << 9;
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position, Vector2.down, math.INFINITY, layer);
        var distance = (hit.point - playerRb2D.position).magnitude;
        grounded = distance > 0 && distance <= (playerCollider.bounds.size.y * 0.55f);

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
}
