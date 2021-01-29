using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the generation of a dungeon floors
/// by Zayarmoe Kyaw 
/// </summary>
public class DungeonManager : MonoBehaviour
{
    [Header("Floor Settings")]
    [SerializeField] private int seed;
    [SerializeField] private bool useSeed = true;
    [SerializeField] private int minRoomsSpawned;
    [SerializeField] private int maxRoomsSpawned;
    [SerializeField] private float floorDensity = 0.5f;
    [SerializeField] private Vector2Int maxFloorSize;
    [SerializeField] private int aproxDistanceRoomsX = 10;
    [SerializeField] private int aproxDistanceRoomsY = 10;

    [Header("RoomPrefabs")]
    [SerializeField] private RoomPrefabManager prefabManager;
    [SerializeField] private GameObject[] spawnableRooms;
    [SerializeField] private GameObject startSpawnRoom;

    [Header("Debug")]
    [SerializeField] private bool debugTextoutput;
    [SerializeField] private Text debugOutput;
    [SerializeField] private bool createRooms = false;
    [SerializeField] private BoxCollider2D raycastPlane;
    [SerializeField] private bool LodActive = false;

    private Vector2Int startRoomPos;
    private int?[,] floorMap;
    private DungeonRoom[,] spawnedFloor;
    private int spawnedRooms = 0;
    private Stack<Vector2Int> spawnBuffer = new Stack<Vector2Int>();
    private System.Random rand;

    void Start()
    {
        spawnableRooms = Resources.LoadAll<GameObject>("RoomPrefabs");
        raycastPlane.size = new Vector2(maxFloorSize.x * (aproxDistanceRoomsX + 1),maxFloorSize.y * (aproxDistanceRoomsY + 1));

        rand = new System.Random();
        if (useSeed)
            rand = new System.Random(seed);

        do
        {
            spawnFloorMath(maxFloorSize);

            if (spawnedRooms <= minRoomsSpawned)
            {
                foreach (var room in spawnedFloor)
                    if (room != null)
                        GameObject.Destroy(room.gameObject);
            }

        } while (spawnedRooms <= minRoomsSpawned);

        if (debugTextoutput)
            printMapAsText();

        if (createRooms)
            foreach (var room in spawnedFloor)
                if (room != null)
                    room.setTeleporters();
    }

    void Update()
    {

    }

    void spawnFloorMath(Vector2Int floorSize)
    {
        spawnedRooms = 0;
        floorMap = new int?[floorSize.x, floorSize.y];
        spawnedFloor = new DungeonRoom[floorSize.x, floorSize.y];

        startRoomPos = new Vector2Int(floorSize.x / 2, floorSize.y / 2);
        floorMap[floorSize.x / 2, floorSize.y / 2] = 9;
        spawnedRooms++;

        if (createRooms)
        {
            var room = Instantiate(startSpawnRoom, Vector3.zero, startSpawnRoom.transform.rotation, this.transform);
            spawnedFloor[floorSize.x / 2, floorSize.y / 2] = room.GetComponent<DungeonRoom>();
            spawnedFloor[floorSize.x / 2, floorSize.y / 2].SetRoomID(startRoomPos.x, startRoomPos.y);
            spawnedFloor[floorSize.x / 2, floorSize.y / 2].SetLod(LodActive);
        }

        createSurroundingRooms(startRoomPos.x, startRoomPos.y);
    }

    void createSurroundingRooms(int roomPosX, int roomPosY)
    {
        if (spawnedRooms >= maxRoomsSpawned)
            return;

        if (roomPosX - 1 >= 0 && floorMap[roomPosX - 1, roomPosY] == null)
            createRoom(roomPosX - 1, roomPosY);

        if (roomPosX + 1 <= maxFloorSize.x - 1 && floorMap[roomPosX + 1, roomPosY] == null)
            createRoom(roomPosX + 1, roomPosY);

        if (roomPosY - 1 >= 0 && floorMap[roomPosX, roomPosY - 1] == null)
            createRoom(roomPosX, roomPosY - 1);

        if (roomPosY + 1 <= maxFloorSize.y - 1 && floorMap[roomPosX, roomPosY + 1] == null)
            createRoom(roomPosX, roomPosY + 1);

        while (spawnBuffer.Count != 0)
        {
            var nextRoom = spawnBuffer.Pop();
            createSurroundingRooms(nextRoom.x, nextRoom.y);
        }

    }

    void createRoom(int roomPosX, int roomPosY)
    {
        if (spawnedRooms == maxRoomsSpawned)
            return;

        floorMap[roomPosX, roomPosY] = ChooseRandom(0, rand.Next(1, spawnableRooms.GetLength(0) + 1), floorDensity);

        if (floorMap[roomPosX, roomPosY] != 0)
        {
            spawnedRooms++;
            spawnBuffer.Push(new Vector2Int(roomPosX, roomPosY));

            if (createRooms)
                createDungeonRoom(roomPosX, roomPosY, floorMap[roomPosX, roomPosY].Value, roomPosX, roomPosY);
        }
    }

    void createDungeonRoom(int x, int y, int roomType, int posX, int posY)
    {
        var room = Instantiate(spawnableRooms[roomType - 1], calculatePosition(posX, posY),
            spawnableRooms[roomType - 1].transform.rotation, this.transform);

        spawnedFloor[posX, posY] = room.GetComponent<DungeonRoom>();
        spawnedFloor[posX, posY].SetRoomID(x, y);
        spawnedFloor[posX, posY].SetLod(LodActive);
    }

    Vector2 calculatePosition(int x, int y)
    {
        var xDif = x - startRoomPos.x;
        var yDif = y - startRoomPos.y;

        var xPos = aproxDistanceRoomsX * xDif;
        var yPos = aproxDistanceRoomsY * yDif;

        return new Vector2(xPos, yPos);
    }

    int checkNeighborCount(int x, int y)
    {
        int neighborCount = 0;

        if (x - 1 >= 0 && floorMap[x - 1, y] != null)
            neighborCount++;
        if (x + 1 <= maxFloorSize.x - 1 && floorMap[x + 1, y] != null)
            neighborCount++;
        if (y - 1 >= 0 && floorMap[x, y - 1] != null)
            neighborCount++;
        if (y + 1 <= maxFloorSize.y - 1 && floorMap[x, y + 1] != null)
            neighborCount++;

        return neighborCount;
    }

    ID[] getEndRooms()
    {
        List<ID> endRooms = new List<ID>();

        for (int x = 0; x < floorMap.GetLength(0); x++)
        {
            for (int y = 0; y < floorMap.GetLength(1); y++)
            {
                if (checkNeighborCount(x, y) == 1)
                    endRooms.Add(new ID(x, y));
            }
        }

        return endRooms.ToArray();
    }

    int getArrayDistance(Vector2Int pos1, Vector2Int pos2)
    {
        return (pos1 - pos2).sqrMagnitude;
    }

    public DungeonRoom getRoomByID(int x, int y)
    {
        if (x >= 0 && y >= 0 && x <= maxFloorSize.x - 1 && y <= maxFloorSize.y - 1)
            return spawnedFloor[x, y];

        return null;
    }

    void printMapAsText()
    {
        debugOutput.text = "";

        for (int i = 0; i < maxFloorSize.x; i++)
        {
            debugOutput.text += "\n";
            for (int j = 0; j < maxFloorSize.y; j++)
            {
                if (floorMap[i, j] == null && floorMap[i, j] != 0)
                    debugOutput.text += " - ";
                else
                    debugOutput.text += " " + floorMap[i, j];
            }
        }
    }

    public int ChooseRandom(int valueOne, int valueTwo, float chance, bool useUnity = false)
    {
        chance = chance * 10;

        UnityEngine.Random.InitState(seed);
        if (UnityEngine.Random.Range(1, 11) > chance && useUnity)
            return valueOne;

        if (rand.Next(1, 11) > chance)
            return valueOne;

        return valueTwo;
    }
}
