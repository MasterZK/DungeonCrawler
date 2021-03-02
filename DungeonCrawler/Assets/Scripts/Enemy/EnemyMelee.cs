using System;
using Unity.Mathematics;
using UnityEngine;

public class EnemyMelee : EnemyBehaviour
{
    [Header("Enemy Attributes")]
    [SerializeField] private float deaccelerationMultiplier = 0.05f;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (start < (1 / spawnSpeed))
        {
            start += Time.deltaTime;
            return;
        }

        move();
        velocityCancel();
        ground();
        if (verticalCheck() > -1.0f && math.abs(horizontalCheck()) < 2.0f)
            jump();
    }

    protected override void move()
    {
        if (horizontalCheckAbs() > 0.9f || horizontalCheckAbs() < -0.9f)
            enemyRb.AddForce(new Vector2((horizontalCheckAbs() * movementSpeed * 10 * Time.deltaTime) * targetAccuracy,0.0f), ForceMode2D.Impulse);

        if (enemyRb.velocity.y < 0)
            enemyRb.velocity += Vector2.up * Physics2D.gravity * 3 * Time.deltaTime;
    }

    protected override void jump()
    {
        if (grounded)
            if (verticalCheck() > 5.0f)
                enemyRb.AddForce((Vector2.up * jumpVelocity * 5 * Time.deltaTime) * targetAccuracy, ForceMode2D.Impulse);
            else
                enemyRb.AddForce((Vector2.up * jumpVelocity * Time.deltaTime) * targetAccuracy, ForceMode2D.Impulse);
    }

    protected override void ground()
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
}
