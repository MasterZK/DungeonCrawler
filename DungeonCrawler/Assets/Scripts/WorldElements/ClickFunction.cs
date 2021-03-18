using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class ClickFunction : MonoBehaviour
{
    [SerializeField] private UnityEvent functionsToCall;

    private void OnMouseDown()
    {
        functionsToCall.Invoke();
    }
}
