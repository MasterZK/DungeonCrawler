using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class TriggerFunction : MonoBehaviour
{
    [SerializeField] private UnityEvent functionsToCall;

    private void OnParticleCollision(GameObject other)
    {
        functionsToCall.Invoke();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        functionsToCall.Invoke();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        functionsToCall.Invoke();
    }
}
