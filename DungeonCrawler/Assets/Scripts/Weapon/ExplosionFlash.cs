using DG.Tweening;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class ExplosionFlash : MonoBehaviour
{
    [SerializeField] private Light2D explosionLight;
    [SerializeField] private float explosionFlashIntensity;

    void Start()
    {
        DOVirtual.Float(0, explosionFlashIntensity, .05f, changeLight).OnComplete(() 
            => DOVirtual.Float(explosionFlashIntensity, 0, .1f, changeLight));
    }

    void changeLight(float x)
    {
        explosionLight.intensity = x;
    }
}
