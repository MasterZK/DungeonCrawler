using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DoorTeleporter : MonoBehaviour
{
    [SerializeField] private Vector3 destinationPosition;
    [SerializeField] public Transform spawnPoint;
    [SerializeField] private Image screenFader;
    [SerializeField] private float transitionSpeed = 0.40f;
    [SerializeField] private GameObject connectedPlatform;

    void Start()
    {
        screenFader = GameObject.Find("ScreenFade").GetComponent<Image>();

        if (connectedPlatform.GetComponent<Platform>().GetPlatformType() && destinationPosition == Vector3.zero)
        {
            this.GetComponent<Collider2D>().isTrigger = false;
            connectedPlatform.GetComponent<Effector2D>().enabled = false;
                Destroy(connectedPlatform.GetComponent<Platform>());
        }
    }

    void Update()
    {
        if (destinationPosition != Vector3.zero && !connectedPlatform.GetComponent<Platform>())
        {
            this.GetComponent<Collider2D>().isTrigger = true;
            connectedPlatform.GetComponent<Effector2D>().enabled = true;
            if (!connectedPlatform.GetComponent<Platform>())
                connectedPlatform.AddComponent<Platform>();
        }
    }

    public void SetTeleportDestination(Vector3 destination)
    {
        destinationPosition = destination;

        this.GetComponent<Collider2D>().isTrigger = true;
        connectedPlatform.GetComponent<Effector2D>().enabled = true;
        if (!connectedPlatform.GetComponent<Platform>())
            connectedPlatform.AddComponent<Platform>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (destinationPosition == Vector3.zero)
            return;

        if (other.gameObject.CompareTag("Player"))
        {
            if (other.gameObject.GetComponent<PlayerMovement2D>().transitioning)
                return;

            if (screenFader)
                StartCoroutine(ScreenFade());
            StartCoroutine(MovePlayer(other));
        }
    }

    IEnumerator MovePlayer(Collider2D player)
    {
        player.GetComponent<Collider2D>().isTrigger = true;
        player.GetComponent<PlayerMovement2D>().transitioning = true;
        var tween = player.GetComponent<Rigidbody2D>().DOMove(destinationPosition, transitionSpeed);
        yield return tween.WaitForCompletion();
        player.GetComponent<PlayerMovement2D>().transitioning = false;
        player.GetComponent<Collider2D>().isTrigger = false;
        player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
    }

    IEnumerator ScreenFade()
    {
        var fading = screenFader.DOFade(100.0f, transitionSpeed / 2);
        yield return fading.WaitForCompletion();
        screenFader.DOFade(0.0f, transitionSpeed / 2);
    }
}
