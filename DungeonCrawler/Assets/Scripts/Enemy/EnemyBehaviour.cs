using System;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(Actor))]
public abstract class EnemyBehaviour : MonoBehaviour
{
    [Header("Enemy Attributes")]
    [SerializeField] protected float movementSpeed = 45;
    [SerializeField] protected float jumpVelocity = 100;
    [SerializeField] private float deaccelerationMultiplier = 0.05f;
    [SerializeField] protected float targetAccuracy = 1;

    [Header("Debug")]
    [SerializeField] protected bool grounded;

    [Header("Animation")]
    [SerializeField] protected float spawnSpeed = 0.5f;

    protected float start = 0;
    public ID currentRoom;

    protected Rigidbody2D enemyRb;
    protected Collider2D enemyCollider;
    protected Animator enemyAnimator;
    protected Actor enemyActor;

    protected PlayerUI player;

    protected virtual void Awake()
    {
        enemyActor = GetComponent<Actor>();

        enemyCollider = this.GetComponent<Collider2D>();
        GetComponent<Renderer>().material = Instantiate(GetComponent<Renderer>().material);

        enemyRb = this.GetComponent<Rigidbody2D>();
        enemyRb.gravityScale = 0;
        enemyRb.constraints = RigidbodyConstraints2D.FreezeRotation;

        enemyAnimator = this.GetComponent<Animator>();
        enemyAnimator.SetFloat("animationSpeed", spawnSpeed);

        this.gameObject.tag = "Hostile";

        spawnAnimation();
    }

    protected virtual void Start()
    {
        player = GameObject.FindObjectOfType<PlayerUI>();

        this.gameObject.tag = "Hostile";
    }

    protected virtual void FixedUpdate()
    {
        if (enemyRb.gravityScale != 1.0f)
            return;
        if (player.CurrentRoom != this.currentRoom)
            return;

        Behaviour();
    }

    protected abstract void Behaviour();

    private void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("Player"))
            enemyActor.Damage(other.GetComponent<Weapon>().controller.GetDPS());
    }

    protected async void spawnAnimation()
    {
        enemyAnimator.Play("SpawnAnimation");

        await WaitForSeconds(1 / spawnSpeed);
        enemyRb.gravityScale = 1;
    }

    protected async Task WaitForSeconds(float seconds)
    {
        await Task.Delay(TimeSpan.FromSeconds(seconds));
    }

    //TODO rewrite for diffrent enemy types
    protected void move()
    {
        if (horizontalCheckAbs() > 0.9f || horizontalCheckAbs() < -0.9f)
            enemyRb.AddForce(new Vector2((horizontalCheckAbs() * movementSpeed * 10 * Time.deltaTime) * targetAccuracy, 0.0f), ForceMode2D.Impulse);

        if (enemyRb.velocity.y < 0)
            enemyRb.velocity += Vector2.up * Physics2D.gravity * 3 * Time.deltaTime;
    }

    protected void jump()
    {
        if (grounded)
            if (verticalCheck() > 5.0f)
                enemyRb.AddForce((Vector2.up * jumpVelocity * 5 * Time.deltaTime) * targetAccuracy, ForceMode2D.Impulse);
            else
                enemyRb.AddForce((Vector2.up * jumpVelocity * Time.deltaTime) * targetAccuracy, ForceMode2D.Impulse);
    }

    protected void ground()
    {
        int layer = 1 << 9;
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position, Vector2.down, math.INFINITY, layer);
        var distance = (hit.point - enemyRb.position).magnitude;
        grounded = distance > 0 && distance <= (enemyCollider.bounds.size.y * 0.55f);
    }

    protected void velocityCancel()
    {
        if (enemyRb.velocity.x < -0.1f)
            enemyRb.velocity += Vector2.right * movementSpeed * deaccelerationMultiplier * Time.deltaTime;
        else if (enemyRb.velocity.x > 0.1f)
            enemyRb.velocity -= Vector2.right * movementSpeed * deaccelerationMultiplier * Time.deltaTime;
        else if (-0.1f < enemyRb.velocity.x && enemyRb.velocity.x < 0.1f)
            enemyRb.velocity = new Vector2(0, enemyRb.velocity.y);

    }

    protected float horizontalCheck()
    {
        var distance = math.abs(player.transform.position.x - this.transform.position.x);

        if (player.transform.position.x < this.transform.position.x)
            return -distance;

        return distance;
    }

    protected float horizontalCheckAbs() => horizontalCheck() / math.abs(horizontalCheck());

    protected float verticalCheck()
    {
        var distance = math.abs(player.transform.position.y - this.transform.position.y);

        if (player.transform.position.y < this.transform.position.y)
            return -distance;

        return distance;
    }

    protected float verticalCheckAbs() => verticalCheck() / math.abs(verticalCheck());

    public void SetRoom(int2 room) => currentRoom = room;
}
