using Unity.Mathematics;
using UnityEngine;

public class EnemyMelee : EnemyBehaviour
{
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
}
