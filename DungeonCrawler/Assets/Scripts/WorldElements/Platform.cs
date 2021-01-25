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
                other.rigidbody.DOMoveY(player.transform.position.y - this.GetComponent<BoxCollider2D>().size.y * 1.2f, 0.5f);
        }
    }

}
