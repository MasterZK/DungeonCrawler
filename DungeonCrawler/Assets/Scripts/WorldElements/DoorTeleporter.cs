using System.Collections;
using DG.Tweening;
using UnityEngine;

public class DoorTeleporter : MonoBehaviour
{
    [SerializeField] public Transform spawnPoint;
    [SerializeField] private Vector3 destinationPosition;
    [SerializeField] private double transitionTime = 0.40f;
    [SerializeField] private GameObject connectedPlatform;

    [SerializeField] private ScreenFade screenFader;
    [SerializeField] private Transform targetManuel;

    void Awake()
    {
        screenFader = GameObject.Find("ScreenFade").GetComponent<ScreenFade>();

        if (destinationPosition != Vector3.zero || targetManuel != null)
        {
            destinationPosition = targetManuel.position;
            return;
        }

        this.GetComponent<Collider2D>().isTrigger = false;
        if (connectedPlatform.GetComponent<Platform>().GetPlatformType())
        {
            connectedPlatform.GetComponent<Effector2D>().enabled = false;
            Destroy(connectedPlatform.GetComponent<Platform>());
        }
    }

    public void SetTeleportDestination(Vector3 destination)
    {
        destinationPosition = destination;

        if (destination == Vector3.zero)
            return;

        this.GetComponent<Collider2D>().isTrigger = true;
        connectedPlatform.GetComponent<Effector2D>().enabled = true;
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
                screenFader.DoScreenFade(transitionTime);
            StartCoroutine(MovePlayer(other));
        }
    }

    IEnumerator MovePlayer(Collider2D player)
    {
        var playerRb = player.GetComponent<Rigidbody2D>();

        playerRb.velocity = Vector2.zero;
        player.isTrigger = true;
        player.GetComponent<PlayerMovement2D>().transitioning = true;

        var tween = playerRb.DOMove(destinationPosition, (float) transitionTime);
        yield return tween.WaitForCompletion();

        player.GetComponent<PlayerMovement2D>().transitioning = false;
        player.isTrigger = false;
        playerRb.velocity = Vector2.zero;
    }
}
