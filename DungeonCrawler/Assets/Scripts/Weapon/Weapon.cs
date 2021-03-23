using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private ParticleSystem part;
    [SerializeField] private GameObject projectileImpact;
    [SerializeField] private float impactForce = 10;
    [SerializeField] public WeaponController controller;

    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

    void Start()
    {
        part = GetComponent<ParticleSystem>();
    }

    void OnParticleCollision(GameObject other)
    {
        part.GetCollisionEvents(other, collisionEvents);
        Instantiate(projectileImpact, collisionEvents[0].intersection, Quaternion.identity);

        Camera.main.DOComplete();
        Camera.main.DOShakePosition(0.1f, 0.1f);

        if (other.GetComponent<Rigidbody2D>() != null && other.CompareTag("Hostile"))
            other.GetComponent<Rigidbody2D>().AddForceAtPosition((collisionEvents[0].intersection - transform.position) * impactForce, collisionEvents[0].intersection, ForceMode2D.Force);

    }
}
