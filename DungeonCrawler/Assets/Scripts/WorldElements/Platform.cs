using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;


public class Platform : MonoBehaviour
{

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
        Vector3 currentVelocity = player.velocity.normalized;
        var move = player.DOMove(player.transform.position + 
                                 (Vector3.down + currentVelocity) * this.GetComponent<BoxCollider2D>().size.y, 0.1f);
        yield return move.WaitForCompletion();
        player.GetComponent<Collider2D>().isTrigger = false;
    }

}
