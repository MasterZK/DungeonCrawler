using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class ActivationZone : MonoBehaviour
{
    [SerializeField] string activatorTag = null;
    [SerializeField] bool deactivateOnExit = false;
    [SerializeField] GameObject[] objects = null;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(activatorTag))
            for (int i = 0; i < objects.Length; i++)
                objects[i].SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (deactivateOnExit && collision.CompareTag(activatorTag))
            for (int i = 0; i < objects.Length; i++)
                objects[i].SetActive(false);
    }
}
