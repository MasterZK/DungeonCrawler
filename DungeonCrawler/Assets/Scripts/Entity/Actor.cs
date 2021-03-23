using UnityEngine;

public class Actor : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float framesFlashing = 7f;
    [Space]
    [SerializeField] private float currentHealth;

    [Header("VFX")] 
    [SerializeField] private Color damagedColor;
    [SerializeField] private GameObject deathFx;

    private SpriteRenderer sprite;
    protected Color actorColor;

    protected void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        actorColor = sprite.color;
        currentHealth = maxHealth;
    }

    public void Damage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
            kill();
        
        damagedVFX();
    }

    private void damagedVFX()
    {
        sprite.color = damagedColor;
        Invoke("resetColor", Time.deltaTime * framesFlashing);
    }

    private void resetColor()
    {
        sprite.color = actorColor;
    }

    public void kill()
    {
        if (gameObject.name == "Player")
        {
            //player death sfx
            CameraShake.ShakeOnce(0.6f, 1.4f);
        }
        else
        {
            //enemy death sfx
        }

        currentHealth = -1;

        /*
        var deathVFX = Instantiate(deathFx, transform.position, transform.rotation);
        ParticleSystem ps = deathVFX.GetComponent<ParticleSystem>();
        var mainPs = ps.main;
        mainPs.startColor = actorColor;
        */

        OnDeath();
        Destroy(gameObject);
    }

    private void OnDeath()
    {

    }

}
