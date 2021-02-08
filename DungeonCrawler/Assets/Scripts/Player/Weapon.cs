using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private ParticleSystem part;
    [SerializeField] private CinemachineImpulseSource screenShake;
    [SerializeField] private GameObject projectileImpact;

    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

    void Start()
    {
        part = GetComponent<ParticleSystem>();
        screenShake = GameObject.FindObjectOfType<CinemachineImpulseSource>();
    }

    void OnParticleCollision(GameObject other)
    {
        part.GetCollisionEvents(other, collisionEvents);
        Instantiate(projectileImpact, collisionEvents[0].intersection, Quaternion.identity);
        screenShake.GenerateImpulse();

        if (other.GetComponent<Rigidbody2D>() != null)
            other.GetComponent<Rigidbody2D>().AddForceAtPosition(collisionEvents[0].intersection * 10 - transform.position, collisionEvents[0].intersection + Vector3.up);

    }
}
