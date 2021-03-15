using System.Collections;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;

public class Platform : MonoBehaviour
{
    [SerializeField] private bool transitionPlatform = false;

    public bool GetPlatformType() => transitionPlatform;

    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            var player = other.gameObject.GetComponent<PlayerMovement2D>();
            if (player.getCrouching() && player.getGrounded())
                StartCoroutine(MovePlayer(player.GetComponent<Rigidbody2D>()));
        }
    }

    IEnumerator MovePlayer(Rigidbody2D player)
    {
        player.GetComponent<Collider2D>().isTrigger = true;

        var move = player.DOMove(checkMovePosition(player), 0.1f);
        yield return move.WaitForCompletion();

        if (!transitionPlatform)
            player.GetComponent<Collider2D>().isTrigger = false;
    }

    Vector2 checkMovePosition(Rigidbody2D player)
    {
        Vector3 currentVelocity = player.velocity.normalized;
        var currentTarget = player.transform.position + ((Vector3.down + currentVelocity) * this.GetComponent<BoxCollider2D>().size.y);
        int layer = 1 << 9;
        bool hit = Physics2D.OverlapCircle(currentTarget,0.01f,layer,0.0f);

        if (hit)
            currentTarget = player.transform.position + (Vector3.down * this.GetComponent<BoxCollider2D>().size.y);

        return currentTarget;
    }

}
