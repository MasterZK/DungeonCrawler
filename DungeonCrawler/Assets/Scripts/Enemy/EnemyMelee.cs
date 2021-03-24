using System;
using Unity.Mathematics;
using UnityEngine;

public class EnemyMelee : EnemyBehaviour
{
    [Header("Enemy Attributes")]
    [SerializeField] private float damage = 5;

    protected override void Behaviour()
    {
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

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player"))
            return;

        other.gameObject.GetComponent<Actor>().Damage(this.damage);
    }
}
