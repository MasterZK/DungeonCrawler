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

    private Effector2D platformEffector2D;
    private Collider2D collider;

    void Awake()
    {
        screenFader = GameObject.Find("ScreenFade").GetComponent<Image>();
        collider = this.GetComponent<Collider2D>();

        if (connectedPlatform)
            platformEffector2D = connectedPlatform.GetComponent<Effector2D>();
    }

    void Update()
    {
        collider.isTrigger = true;
        if (connectedPlatform)
        {
            platformEffector2D.enabled = true;
            if (!connectedPlatform.GetComponent<Platform>())
                connectedPlatform.AddComponent<Platform>();
        }

        if (destinationPosition == Vector3.zero)
        {
            collider.isTrigger = false;
            if (connectedPlatform)
            {
                platformEffector2D.enabled = false;
                Destroy(connectedPlatform.GetComponent<Platform>());
            }
        }
    }

    public void SetTeleportDestination(Vector3 destination)
    {
        destinationPosition = destination;
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
    }

    IEnumerator ScreenFade()
    {
        var fading = screenFader.DOFade(100.0f, transitionSpeed / 2);
        yield return fading.WaitForCompletion();
        screenFader.DOFade(0.0f, transitionSpeed / 2);
    }
}
