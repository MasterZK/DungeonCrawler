using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;

public struct RoomType
{

}

/// <summary>
/// Manages the generation of a dungeon floors
/// by Zayarmoe Kyaw 
/// </summary>
public class DungeonManager : MonoBehaviour
{
    [Header("Floor Settings")]
    [SerializeField] private int seed;
    [SerializeField] private bool useSeed = true;

    [Range(0.0f, 1.0f)]
    [SerializeField] private float floorDensity = 0.5f;
    [SerializeField] private int2 maxFloorSize;
    [SerializeField] private int minRoomsSpawned;
    [SerializeField] private int maxRoomsSpawned;

    [SerializeField] private int approxDistanceRoomsX = 25;
    [SerializeField] private int approxDistanceRoomsY = 15;

    [Header("RoomPrefabs")]
    [SerializeField] private RoomPrefabManager prefabManager;
    [SerializeField] private GameObject[] spawnableRooms;
    [SerializeField] private GameObject startSpawnRoom;

    [Header("Debug")]
    [SerializeField] private BoxCollider2D raycastPlane;
    [SerializeField] private bool createAstar = false;
    [SerializeField] private bool createRooms = false;

    [SerializeField] private bool LodActive = false;
    [SerializeField] private bool debugSetActive = false;

    [SerializeField] private bool debugTextoutput = false;
    [SerializeField] private Text debugOutput;

    private DungeonGrid dungeonFloor;
    private List<ID> spawnedRoomIDs;
    private AStarPathFinder aStarCalculator;
    [ItemCanBeNull] private DungeonRoom[,] spawnedFloor;

    private int spawnedRooms = 0;
    private Stack<Vector2Int> spawnBuffer = new Stack<Vector2Int>();
    private System.Random rand;


    void Start()
    {
        //float startTime = Time.realtimeSinceStartup;

        spawnableRooms = Resources.LoadAll<GameObject>("RoomPrefabs");
        raycastPlane.size = new Vector2(maxFloorSize.x * (approxDistanceRoomsX + 1), maxFloorSize.y * (approxDistanceRoomsY + 1));

        rand = new System.Random();
        if (useSeed)
            rand = new System.Random(seed);

        if (createRooms)
            createAstar = createRooms;

        do
        {
            spawnFloorMath();
        } while (spawnedRooms <= minRoomsSpawned);

        if (createAstar)
            createAStarFloor();
        
        if (createRooms)
        {
            createFloor();

            foreach (var room in spawnedFloor)
                if (room != null)
                    room.setTeleporters();
        }

        //Debug.Log("Time: " + ((Time.realtimeSinceStartup - startTime) * 1000f));

        if (debugTextoutput)
            printMapAsText();
    }

    private void OnDestroy()
    {
        dungeonFloor.Dispose();
    }

    void spawnFloorMath()
    {
        spawnedRooms = 0;
        if (minRoomsSpawned >= maxRoomsSpawned)
            minRoomsSpawned = maxRoomsSpawned - 10;

        dungeonFloor = new DungeonGrid(maxFloorSize, seed, useSeed);
        spawnedRoomIDs = new List<ID>();

        var startID = dungeonFloor.startRoomPos;
        dungeonFloor.floorMap[dungeonFloor.IDToIndex(startID)] = new DungeonRoomAStar(startID, false);
        spawnedRoomIDs.Add(startID);
        spawnedRooms++;

        createSurroundingRooms(startID.x, startID.y);
    }

    void createSurroundingRooms(int roomPosX, int roomPosY)
    {
        if (spawnedRooms >= maxRoomsSpawned)
            return;

        if (roomPosX - 1 >= 0 && !dungeonFloor.floorMap[dungeonFloor.IDToIndex(roomPosX - 1, roomPosY)].created)
            createRoom(roomPosX - 1, roomPosY);

        if (roomPosX + 1 <= maxFloorSize.x - 1 && !dungeonFloor.floorMap[dungeonFloor.IDToIndex(roomPosX + 1, roomPosY)].created)
            createRoom(roomPosX + 1, roomPosY);

        if (roomPosY - 1 >= 0 && !dungeonFloor.floorMap[dungeonFloor.IDToIndex(roomPosX, roomPosY - 1)].created)
            createRoom(roomPosX, roomPosY - 1);

        if (roomPosY + 1 <= maxFloorSize.y - 1 && !dungeonFloor.floorMap[dungeonFloor.IDToIndex(roomPosX, roomPosY + 1)].created)
            createRoom(roomPosX, roomPosY + 1);

        while (spawnBuffer.Count != 0)
        {
            var nextRoom = spawnBuffer.Pop();
            createSurroundingRooms(nextRoom.x, nextRoom.y);
        }
    }

    void createRoom(int roomPosX, int roomPosY)
    {
        if (spawnedRooms >= maxRoomsSpawned)
            return;

        var index = dungeonFloor.IDToIndex(roomPosX, roomPosY);
        dungeonFloor.floorMap[index] = createRoom(roomPosX, roomPosY, floorDensity);

        if (!dungeonFloor.floorMap[index].Null && dungeonFloor.floorMap[index].created)
        {
            spawnedRoomIDs.Add(new ID(roomPosX, roomPosY));
            spawnBuffer.Push(new Vector2Int(roomPosX, roomPosY));
            spawnedRooms++;
        }
    }

    DungeonRoomAStar createRoom(ID roomID, float chance)
    {
        var result = chooseRandom(0, 1, chance);
        return new DungeonRoomAStar(roomID, result != 1);
    }

    DungeonRoomAStar createRoom(int x, int y, float chance) => createRoom(new ID(x, y), chance);

    int chooseRandom(int valueOne, int valueTwo, float chance)
    {
        chance *= 10;
        if (rand.Next(1, 11) > chance)
            return valueOne;

        return valueTwo;
    }

    public void createAStarFloor()
    {
        if (!this.GetComponent<AStarPathFinder>())
            this.gameObject.AddComponent<AStarPathFinder>();

        aStarCalculator = gameObject.GetComponent<AStarPathFinder>();
        aStarCalculator.SetGrid(dungeonFloor);

        for (int i = 0; i < spawnedRoomIDs.Count; i++)
        {
            List<AStarSearchCall> aStarRequests = new List<AStarSearchCall>();

            for (int j = 0; j < JobsUtility.JobWorkerMaximumCount; j++)
            {
                if (spawnedRoomIDs.Count < i + 1)
                    break;

                var astar = new AStarSearchCall(dungeonFloor.startRoomPos, spawnedRoomIDs[i]);
                aStarRequests.Add(astar);
                i++;
            }

            aStarCalculator.MassQueueJob(aStarRequests);
            aStarCalculator.MassDequeueJob(JobsUtility.JobWorkerMaximumCount);

            for (int j = 0; j < aStarRequests.Count; j++)
            {
                var roomIndex = dungeonFloor.IDToIndex(aStarRequests[j].endNode);
                dungeonFloor.floorMap[roomIndex] = aStarRequests[j].result;
            }

            i--;
        }
    }

    Vector2 calculatePosition(int x, int y)
    {
        var xDif = x - dungeonFloor.startRoomPos.x;
        var yDif = y - dungeonFloor.startRoomPos.y;

        var xPos = approxDistanceRoomsX * xDif;
        var yPos = approxDistanceRoomsY * yDif;

        return new Vector2(xPos, yPos);
    }

    void createFloor()
    {
        //later done by prefab manager
        spawnedFloor = new DungeonRoom?[maxFloorSize.x, maxFloorSize.y];

        for (int i = 0; i < spawnedRoomIDs.Count; i++)
        {
            var id = spawnedRoomIDs[i];
            createDungeonRoom(id.X, id.Y, 1);
        }

    }

    void createDungeonRoom(int x, int y, int roomType)
    {
        GameObject room;

        if (new ID(x, y) == dungeonFloor.startRoomPos)
            room = Instantiate(startSpawnRoom, calculatePosition(x, y),
                startSpawnRoom.transform.rotation, this.transform);
        else
            room = Instantiate(spawnableRooms[roomType - 1], calculatePosition(x, y),
                spawnableRooms[roomType - 1].transform.rotation, this.transform);

        var dungeonRoom = room.GetComponent<DungeonRoom>();
        dungeonRoom.SetRoomID(x, y);
        dungeonRoom.SetLod(LodActive);

        spawnedFloor[x, y] = dungeonRoom;

        if (new ID(x, y) != dungeonFloor.startRoomPos)
            room.SetActive(!debugSetActive);
    }

    int checkNeighborCount(int x, int y)
    {
        int neighborCount = 0;

        var room = dungeonFloor.floorMap[dungeonFloor.IDToIndex(x - 1, y)];
        if (x - 1 >= 0 && room.created && !room.Null)
            neighborCount++;
        room = dungeonFloor.floorMap[dungeonFloor.IDToIndex(x + 1, y)];
        if (x + 1 <= maxFloorSize.x - 1 && room.created && !room.Null)
            neighborCount++;
        room = dungeonFloor.floorMap[dungeonFloor.IDToIndex(x, y - 1)];
        if (y - 1 >= 0 && room.created && !room.Null)
            neighborCount++;
        room = dungeonFloor.floorMap[dungeonFloor.IDToIndex(x, y + 1)];
        if (y + 1 <= maxFloorSize.y - 1 && room.created && !room.Null)
            neighborCount++;

        return neighborCount;
    }

    ID[] getEndRooms()
    {

        //TODO
        //determine endrooms by Astar and neightbors

        List<ID> endRooms = new List<ID>();

        for (int x = 0; x < maxFloorSize.x; x++)
        {
            for (int y = 0; y < maxFloorSize.y; y++)
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

    public DungeonRoom GetRoomByID(int x, int y)
    {
        if (x >= 0 && y >= 0 && x <= maxFloorSize.x - 1 && y <= maxFloorSize.y - 1)
            return spawnedFloor[x, y];

        return null;
    }

    public DungeonRoom GetRoomByID(ID roomID) => GetRoomByID(roomID.X, roomID.Y);

    void printMapAsText()
    {
        debugOutput.text = "";

        for (int i = 0; i < maxFloorSize.x; i++)
        {
            debugOutput.text += "\n";
            for (int j = 0; j < maxFloorSize.y; j++)
            {
                var room = dungeonFloor.floorMap[dungeonFloor.IDToIndex(i, j)];
                if (!room.Null && room.created)
                    debugOutput.text += " " + "1";
                else
                    debugOutput.text += " - ";
            }
        }

        if (!createAstar)
            return;

        debugOutput.text += "\n Astar map";

        for (int i = 0; i < maxFloorSize.x; i++)
        {
            debugOutput.text += "\n";
            for (int j = 0; j < maxFloorSize.y; j++)
            {
                var room = dungeonFloor.floorMap[dungeonFloor.IDToIndex(i, j)];
                if (!room.Null && room.created)
                    debugOutput.text += " " + room.Cost;
                else
                    debugOutput.text += " - ";
            }
        }

        //for (int i = 0; i < maxFloorSize.x; i++)
        //{
        //    debugOutput.text += "\n";
        //    for (int j = 0; j < maxFloorSize.y; j++)
        //    {
        //        if (spawnedFloorAStar[i, j] == null || floorMap[i, j] == 0)
        //            debugOutput.text += " -  ";
        //        else
        //            debugOutput.text += spawnedFloorAStar[i, j].Cost + "  ";
        //    }
        //}

    }

}
