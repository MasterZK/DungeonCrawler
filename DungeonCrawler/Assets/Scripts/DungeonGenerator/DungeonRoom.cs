using Unity.Mathematics;
using UnityEngine;

public class DungeonRoom : MonoBehaviour
{
    [SerializeField] private float height, width = 0;
    [SerializeField] private DoorTeleporter[] doors = new DoorTeleporter[4];
    [SerializeField] private LODGroup LodRenderGroup;

    private RoomType roomType;
    private ID roomID;
    private DungeonManager dungeonManager;

    void Awake()
    {
        dungeonManager = GameObject.FindObjectOfType<DungeonManager>();
        adjustSize();
    }

    void OnValidate()
    {
        adjustSize();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        Camera.main.transform.SetPositionAndRotation(this.transform.position + new Vector3(0, 0, -10), Quaternion.identity);

        var previousRoom = other.GetComponent<PlayerAttributes>().CurrentRoom;
        setNeighborRooms(previousRoom, false);
        setNeighborRooms(this.roomID, true);
        other.GetComponent<PlayerAttributes>().CurrentRoom = this.roomID;
    }

    private void setNeighborRooms(ID room, bool state)
    {
        setRoom(room + Vector2Int.up, state);
        setRoom(room + Vector2Int.down, state);
        setRoom(room + Vector2Int.left, state);
        setRoom(room + Vector2Int.right, state);
    }

    private void setRoom(ID room, bool state)
    {
        DungeonRoom? tempRoom = null;
        if (room != this.roomID)
            tempRoom = dungeonManager.GetRoomByID(room);

        if (tempRoom != null)
            tempRoom.gameObject.SetActive(state);
    }

    public void SetLod(bool state)
    {
        LodRenderGroup.enabled = state;
    }

    public void setTeleporters()
    {
        var result = dungeonManager.GetRoomByID(roomID.X - 1, roomID.Y);
        if (result != null)
        {
            var teleDestination = result.getTeleporter(1);
            doors[3].SetTeleportDestination(teleDestination.spawnPoint.position);
        }

        result = dungeonManager.GetRoomByID(roomID.X + 1, roomID.Y);
        if (result != null)
        {
            var teleDestination = result.getTeleporter(3);
            doors[1].SetTeleportDestination(teleDestination.spawnPoint.position);
        }

        result = dungeonManager.GetRoomByID(roomID.X, roomID.Y - 1);
        if (result != null)
        {
            var teleDestination = result.getTeleporter(0);
            doors[2].SetTeleportDestination(teleDestination.spawnPoint.position);
        }

        result = dungeonManager.GetRoomByID(roomID.X, roomID.Y + 1);
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

    public void SetRoomID(int xID, int yID)
    {
        roomID = new int2(xID, yID);
        this.gameObject.name = (roomID.X + 1) + " " + (roomID.Y + 1);
    }

    public ID GetRoomID() => roomID;

    public void SetRoomType(RoomType newType) => roomType = newType;

    public RoomType GetRoomType() => roomType;

    private void adjustSize()
    {
        var size = this.GetComponent<Collider2D>().bounds.size;
        height = size.y;
        width = size.x;
    }
}
