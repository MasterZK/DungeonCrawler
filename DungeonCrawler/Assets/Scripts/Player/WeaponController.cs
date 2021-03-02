using System;
using System.Collections.Generic;
using UnityEngine;

struct WeaponStats
{
    public bool automatic;                  //looping & prewarm
    public float projectileRange;           //start lifetime                    //1-0
    public float projectileSpeed;           //start speed
    public float fallOff;                   //gravity modifier                  //1-...

    //bursts
    public short bulletsPerShot;              //count
    public float fireRate;                  //interval                          //1-0.01

    //shape
    public float accuracy;                  //arc & /2 z rotation offset        //0-90
}

public enum WeaponType
{
    Automatic = 0,
    Burst = 1,
    Shotgun = 2,
    Sniper = 3,

}

public class WeaponController : MonoBehaviour
{
    [Header("Weapon Base Attributes")]
    [SerializeField] private Vector2 shootDirection;
    [SerializeField] private float weaponSnapSpeed = 1;
    
    [Header("Projectile")]
    [SerializeField] private ParticleSystem projectiles;
    [SerializeField] private WeaponType currentWeapon = WeaponType.Automatic;
    private Dictionary<WeaponType, WeaponStats> weaponPresets = new Dictionary<WeaponType, WeaponStats>();

    [Header("Custom Cursor")] 
    [SerializeField] private Texture2D customMouseTexture = null;
    [SerializeField] private CursorLockMode cursorMode;


    // Start is called before the first frame update
    void Start()
    {
        if (customMouseTexture != null)
            Cursor.SetCursor(customMouseTexture, new Vector2(customMouseTexture.height / 2, customMouseTexture.width / 2), CursorMode.ForceSoftware);
        Cursor.lockState = cursorMode;

        initWeaponStats();
        setWeapon(currentWeapon,true);
    }

    // Update is called once per frame
    void Update()
    {
        adjustWeaponDirection();
        shootProjectile();
    }

    void initWeaponStats()
    {
        WeaponStats newWeapon = new WeaponStats();

        //automatic rifle
        newWeapon.automatic = true;
        newWeapon.projectileRange = 0.5f;
        newWeapon.projectileSpeed = 30;
        newWeapon.fallOff = 1;

        newWeapon.bulletsPerShot = 1;
        newWeapon.fireRate = 1;
        newWeapon.accuracy = 10;

        weaponPresets.Add(WeaponType.Automatic,newWeapon);

        //shotgun
        newWeapon.automatic = false;
        newWeapon.projectileRange = 0.3f;
        newWeapon.projectileSpeed = 40;
        newWeapon.fallOff = 1.2f;

        newWeapon.bulletsPerShot = 6;
        newWeapon.fireRate = 1;
        newWeapon.accuracy = 30;

        weaponPresets.Add(WeaponType.Shotgun, newWeapon);

    }

    void setWeapon(WeaponType newWeapon, bool overrideCommand = false)
    {
        if (currentWeapon == newWeapon && !overrideCommand)
            return;
        currentWeapon = newWeapon;

        WeaponStats newWeaponStats;
        weaponPresets.TryGetValue(newWeapon, out newWeaponStats);

        var main = projectiles.main;
        main.loop = newWeaponStats.automatic;
        main.prewarm = newWeaponStats.automatic;
        main.startLifetime = newWeaponStats.projectileRange;
        main.startSpeed = newWeaponStats.projectileSpeed;
        main.gravityModifier = newWeaponStats.fallOff;

        var burst = projectiles.emission.GetBurst(0);
        burst.maxCount = burst.minCount = newWeaponStats.bulletsPerShot;
        burst.repeatInterval = newWeaponStats.fireRate;
        projectiles.emission.SetBurst(0,burst);

        var shape = projectiles.shape;
        shape.arc = newWeaponStats.accuracy;
        shape.rotation = new Vector3(0,0,-newWeaponStats.accuracy / 2);

    }

    void shootProjectile()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
            projectiles.Play();
        else if (Input.GetKeyUp(KeyCode.Mouse0))
            projectiles.Stop();
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Weapon"))
        {
            setWeapon((WeaponType) Enum.Parse(typeof(WeaponType), other.name));
            Destroy(other.gameObject);
        }
    }

}

