using Unity.Mathematics;
using UnityEngine;

[RequireComponent( typeof(BoxCollider2D))]
public class HoverObject : MonoBehaviour
{
    [SerializeField] private float colliderSize = 1;
    [SerializeField] private float hoverSpeed = 1;
    [SerializeField] private float hoverDistance = 0.4f;

    [SerializeField] private WeaponType weaponPickup;

    private BoxCollider2D boxColl;

    private Vector2 startPos;
    private Vector2 hoverPos;

    void Start()
    {
        boxColl = this.GetComponent<BoxCollider2D>();
        boxColl.isTrigger = true;
        boxColl.size = new Vector2(colliderSize, colliderSize);

        this.name = weaponPickup.ToString();

        startPos = this.transform.position;
    }

    void Update()
    {
        hover();
    }

    void hover()
    {
        hoverPos = startPos;
        hoverPos.y += math.sin(Time.fixedTime * math.PI * hoverSpeed) * hoverDistance + hoverDistance;

        transform.position = hoverPos;
    }
}
