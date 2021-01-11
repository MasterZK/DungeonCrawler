using System.Collections;
using UnityEngine;

public class DoorTeleporter : MonoBehaviour
{
    [SerializeField] private Transform newPosition;
    [SerializeField] private Animator blackScreen;
    //[SerializeField] private GameObject playerCamera;
    [SerializeField] private float transitionSpeed = 0.40f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            blackScreen.Play(("ScreenFadeON"));
            StartCoroutine((WaitFor(transitionSpeed, other)));
        }
    }

    IEnumerator WaitFor(float seconds, Collider2D other)
    {
        yield return new WaitForSeconds(seconds);
        other.transform.position = newPosition.position;
        //playerCamera.transform.position = newPosition.position;
    }
}
