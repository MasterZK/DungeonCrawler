using System.Collections;
using UnityEngine;

public class DoorTeleporter : MonoBehaviour
{
    [SerializeField] private Vector3 newPosition;
    [SerializeField] public Transform spawnPoint;
    [SerializeField] private Animator blackScreen;
    //[SerializeField] private GameObject playerCamera;
    [SerializeField] private float transitionSpeed = 0.40f;

    void Awake()
    {
        blackScreen = GameObject.Find("ScreenFade").GetComponent<Animator>();
    }

    void Update()
    {
        this.GetComponent<Collider2D>().isTrigger = true;
        if (newPosition == Vector3.zero)
            this.GetComponent<Collider2D>().isTrigger = false;
    }

    public void SetTeleportDestination(Vector3 destination)
    {
        newPosition = destination;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (newPosition == Vector3.zero)
            return;

        if (other.gameObject.CompareTag("Player"))
        {
            blackScreen.Play(("ScreenFadeOn"));
            StartCoroutine((WaitFor(transitionSpeed, other)));
        }
    }

    IEnumerator WaitFor(float seconds, Collider2D other)
    {
        yield return new WaitForSeconds(seconds);
        other.transform.position = newPosition;
        //playerCamera.transform.position = newPosition.position;
    }
}
