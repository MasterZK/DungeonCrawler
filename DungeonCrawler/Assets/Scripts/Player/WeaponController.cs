using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Weapon Base Attributes")]
    [SerializeField] private Vector2 shootDirection;
    [SerializeField] private float weaponSnapSpeed = 1;
    [SerializeField] private float shootDelay = 0.5f;

    [Header("Projectile")]
    [SerializeField] private GameObject baseBullet;
    [SerializeField] private float projectileSpeed = 1;
    [SerializeField] private float spawnOffset = 1;
    [SerializeField] private float projectileLifetime = 1;

    [SerializeField] private ParticleSystem projectiles;

    [Header("Custom Cursor")]
    [SerializeField] private Texture2D customMouseTexture = null;
    [SerializeField] private int cursorSize = 100;
    [SerializeField] private CursorLockMode cursorMode;

    // Start is called before the first frame update
    void Start()
    {
        if (customMouseTexture != null)
            Cursor.SetCursor(customMouseTexture, new Vector2(customMouseTexture.height / 2, customMouseTexture.width / 2), CursorMode.ForceSoftware);
        Cursor.lockState = cursorMode;
    }

    // Update is called once per frame
    void Update()
    {
        adjustWeaponDirection();
        shootProjectile();

        //shoot();
    }

    void shootProjectile()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
            projectiles.Play();
        else if (Input.GetKeyUp(KeyCode.Mouse0))
            projectiles.Stop();
    }

    void shoot()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            var bullet = Instantiate(baseBullet, this.transform.position +
                                                (new Vector3(shootDirection.x, shootDirection.y) * spawnOffset), this.transform.rotation);
            bullet.GetComponent<Rigidbody2D>().AddForce(shootDirection * projectileSpeed, ForceMode2D.Impulse);
            Destroy(bullet, projectileLifetime);
        }
    }

    void adjustWeaponDirection()
    {
        shootDirection = (getMousePosition() - this.transform.position).normalized;

        Quaternion target = Quaternion.Euler(0, 0, Vector3.Angle(Vector3.right, shootDirection));
        if (checkMousePosition(getMousePosition()))
            target = Quaternion.Euler(0, 0, -Vector3.Angle(Vector3.right, shootDirection));

        transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * weaponSnapSpeed);
    }

    Vector3 getMousePosition()
    {
        RaycastHit2D hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        hit = Physics2D.Raycast(ray.origin, ray.direction);

        return hit.point;
    }

    bool checkMousePosition(Vector3 mousePosition)
    {
        if (mousePosition.y < this.transform.position.y)
            return true;

        return false;
    }

}

