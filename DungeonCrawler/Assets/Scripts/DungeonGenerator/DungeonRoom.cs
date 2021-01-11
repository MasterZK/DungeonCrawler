using UnityEngine;

struct ID
{
    private int x, y;
}

public class DungeonRoom : MonoBehaviour
{
    [SerializeField] private int height, width = 0;
    [SerializeField] private GameObject[] doors;

    private ID roomID;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
