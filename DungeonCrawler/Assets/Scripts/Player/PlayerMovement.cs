using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float movementSpeed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.Translate(new Vector2(Input.GetAxisRaw("Horizontal") * Time.deltaTime * movementSpeed,
            Input.GetAxisRaw("Vertical") * Time.deltaTime * movementSpeed));
    }
}
