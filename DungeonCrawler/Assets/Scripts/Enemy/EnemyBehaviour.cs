using System;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public abstract class EnemyBehaviour : MonoBehaviour
{
    [Header("Enemy Attributes")]
    [SerializeField] protected float movementSpeed = 45;
    [SerializeField] protected float jumpVelocity = 100;
    [SerializeField] protected float targetAccuracy = 1;

    [Header("Debug")]
    [SerializeField] protected bool grounded;

    [Header("Animation")]
    [SerializeField] protected float spawnSpeed = 0.5f;

    protected int2 currentRoom;
    protected PlayerAttributes player;
    protected Rigidbody2D enemyRb;
    protected Collider2D enemyCollider;
    protected Animator enemyAnimator;

    protected float start = 0;

    protected virtual void Awake()
    {
        enemyCollider = this.GetComponent<Collider2D>();
        GetComponent<Renderer>().material = Instantiate(GetComponent<Renderer>().material);

        enemyRb = this.GetComponent<Rigidbody2D>();
        enemyRb.gravityScale = 0;
        enemyRb.constraints = RigidbodyConstraints2D.FreezeRotation;

        enemyAnimator = this.GetComponent<Animator>();
        enemyAnimator.SetFloat("animationSpeed", spawnSpeed);

        spawnAnimation();
    }

    protected virtual void Start()
    {
        player = GameObject.FindObjectOfType<PlayerAttributes>();

        this.gameObject.tag = "Hostile";
    }

    protected virtual void Update()
    {

    }

    protected virtual void FixedUpdate()
    {
        if (enemyRb.gravityScale != 1.0f)
            return;

        var compare = player.CurrentRoom != this.currentRoom;
        if (compare.x && compare.y)
            return;
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

    protected abstract void move();

    protected abstract void jump();

    protected abstract void ground();

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
