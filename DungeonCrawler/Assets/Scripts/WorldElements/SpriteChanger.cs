using UnityEngine;

public class SpriteChanger : MonoBehaviour
{
    [SerializeField] private Sprite optionOne;
    [SerializeField] private Sprite optionTwo;

    private SpriteRenderer renderer;
    
    private void Awake()
    {
        renderer = this.GetComponent<SpriteRenderer>();
        renderer.sprite = optionOne;
    }

    public void changeSprite()
    {
        renderer.sprite = renderer.sprite == optionOne ? optionTwo : optionOne;
    }
}
