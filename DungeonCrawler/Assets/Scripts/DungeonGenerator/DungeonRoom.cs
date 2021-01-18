using System.Linq;
using UnityEngine;

public struct ID
{
    public int x { get; }
    public int y { get; }

    public ID(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}

public class DungeonRoom : MonoBehaviour
{
    [SerializeField] private float height, width = 0;
    [SerializeField] private DoorTeleporter[] doors = new DoorTeleporter[4];

    private ID roomID;
    private DungeonManager dungeonManager;

    void Awake()
    {
        dungeonManager = GameObject.FindObjectOfType<DungeonManager>();
        adjustSize();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnValidate()
    {
        adjustSize();
    }

    public void setTeleporters()
    {
        var result = dungeonManager.getRoomByID(roomID.x - 1, roomID.y);
        if (result != null)
        {
            var teleDestination = result.getTeleporter(1);
            doors[3].SetTeleportDestination(teleDestination.spawnPoint.position);
        }

        result = dungeonManager.getRoomByID(roomID.x + 1, roomID.y);
        if (result != null)
        {
            var teleDestination = result.getTeleporter(3);
            doors[1].SetTeleportDestination(teleDestination.spawnPoint.position);
        }

        result = dungeonManager.getRoomByID(roomID.x, roomID.y - 1);
        if (result != null)
        {
            var teleDestination = result.getTeleporter(0);
            doors[2].SetTeleportDestination(teleDestination.spawnPoint.position);
        }

        result = dungeonManager.getRoomByID(roomID.x, roomID.y + 1);
        if (result != null)
        {
            var teleDestination = result.getTeleporter(2);
            doors[0].SetTeleportDestination(teleDestination.spawnPoint.position);
        }

    }

    public DoorTeleporter getTeleporter(int doorIndex)
    {
        return this.doors[doorIndex];
    }

    private void adjustSize()
    {
        var size = this.GetComponent<MeshCollider>().bounds.size;
        height = size.y;
        width = size.x;
    }

    public void SetRoomID(int xID, int yID)
    {
        roomID = new ID(xID, yID);
        this.gameObject.name = (roomID.x + 1) + " " + (roomID.y + 1);
    }

    public ID GetRoomID()
    {
        return roomID;
    }
}
