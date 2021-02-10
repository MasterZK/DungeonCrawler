using System;
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
    [SerializeField] private LODGroup LodRenderGroup;

    public DungeonRoomAStar aStarRoom;

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
        Camera.main.transform.SetPositionAndRotation(this.transform.position + new Vector3(0, 0, -10), Quaternion.identity);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        //TODO
        //disable adjacent rooms and itself when room is left to avoid unnecessary updates 
        throw new NotImplementedException();
    }

    public void SetLod(bool state)
    {
        LodRenderGroup.enabled = state;
    }

    public void setTeleporters()
    {
        var result = dungeonManager.GetRoomByID(roomID.x - 1, roomID.y);
        if (result != null)
        {
            var teleDestination = result.getTeleporter(1);
            doors[3].SetTeleportDestination(teleDestination.spawnPoint.position);
        }

        result = dungeonManager.GetRoomByID(roomID.x + 1, roomID.y);
        if (result != null)
        {
            var teleDestination = result.getTeleporter(3);
            doors[1].SetTeleportDestination(teleDestination.spawnPoint.position);
        }

        result = dungeonManager.GetRoomByID(roomID.x, roomID.y - 1);
        if (result != null)
        {
            var teleDestination = result.getTeleporter(0);
            doors[2].SetTeleportDestination(teleDestination.spawnPoint.position);
        }

        result = dungeonManager.GetRoomByID(roomID.x, roomID.y + 1);
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
        roomID = new ID(xID, yID);
        this.gameObject.name = (roomID.x + 1) + " " + (roomID.y + 1);
    }

    public ID GetRoomID() => roomID;

    private void adjustSize()
    {
        var size = this.GetComponent<Collider2D>().bounds.size;
        height = size.y;
        width = size.x;
    }
}
