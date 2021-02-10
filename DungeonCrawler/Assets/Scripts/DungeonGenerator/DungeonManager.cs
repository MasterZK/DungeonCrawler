using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor.Experimental.GraphView;
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
    [SerializeField] private bool debugTextoutput = false;
    [SerializeField] private bool createAstar = false;
    [SerializeField] private Text debugOutput;
    [SerializeField] private bool createRooms = false;
    [SerializeField] private BoxCollider2D raycastPlane;
    [SerializeField] private bool LodActive = false;

    private Vector2Int startRoomPos;
    [ItemCanBeNull] private int?[,] floorMap;
    [ItemCanBeNull] private DungeonRoomAStar[,] spawnedFloorAStar;
    [ItemCanBeNull] private DungeonRoom[,] spawnedFloor;
    private int spawnedRooms = 0;
    private Stack<Vector2Int> spawnBuffer = new Stack<Vector2Int>();
    private System.Random rand;

    void Start()
    {
        spawnableRooms = Resources.LoadAll<GameObject>("RoomPrefabs");
        raycastPlane.size = new Vector2(maxFloorSize.x * (aproxDistanceRoomsX + 1), maxFloorSize.y * (aproxDistanceRoomsY + 1));
        createAstar = createRooms;

        rand = new System.Random();
        if (useSeed)
            rand = new System.Random(seed);

        do
        {
            spawnFloorMath(maxFloorSize);
        } while (spawnedRooms <= minRoomsSpawned);

        if (createAstar)
            createAstarFloor();

        if (createRooms)
            createFloor();

        if (createRooms)
            foreach (var room in spawnedFloor)
                if (room != null)
                    room.setTeleporters();

        if (debugTextoutput)
            printMapAsText();
    }

    void spawnFloorMath(Vector2Int floorSize)
    {
        spawnedRooms = 0;
        floorMap = new int?[floorSize.x, floorSize.y];
        spawnedFloor = new DungeonRoom[floorSize.x, floorSize.y];

        startRoomPos = new Vector2Int(floorSize.x / 2, floorSize.y / 2);
        floorMap[floorSize.x / 2, floorSize.y / 2] = 9;
        spawnedRooms++;

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

        floorMap[roomPosX, roomPosY] = chooseRandom(0, rand.Next(1, spawnableRooms.GetLength(0) + 1), floorDensity);

        if (floorMap[roomPosX, roomPosY] != 0)
        {
            spawnedRooms++;
            spawnBuffer.Push(new Vector2Int(roomPosX, roomPosY));
        }
    }

    int chooseRandom(int valueOne, int valueTwo, float chance, bool useUnity = false)
    {
        chance = chance * 10;

        UnityEngine.Random.InitState(seed);
        if (UnityEngine.Random.Range(1, 11) > chance && useUnity)
            return valueOne;

        if (rand.Next(1, 11) > chance)
            return valueOne;

        return valueTwo;
    }

    void createAstarFloor()
    {
        spawnedFloorAStar = new DungeonRoomAStar[maxFloorSize.x, maxFloorSize.y];

        for (int x = 0; x < maxFloorSize.x; x++)
            for (int y = 0; y < maxFloorSize.y; y++)
                if (floorMap[x, y] != null && floorMap[x, y] != 0)
                {
                    spawnedFloorAStar[x, y] = new DungeonRoomAStar();
                    spawnedFloorAStar[x, y].SetRoomID(x, y);
                }

        for (int x = 0; x < maxFloorSize.x; x++)
            for (int y = 0; y < maxFloorSize.y; y++)
                calculateAStar(new ID(x, y), new ID(maxFloorSize.x / 2, maxFloorSize.y / 2));

    }

    Stack<DungeonRoomAStar> calculateAStar(ID destination, ID start, bool returnShortest = false)
    {
        Stack<DungeonRoomAStar> path = new Stack<DungeonRoomAStar>();
        List<DungeonRoomAStar> openList = new List<DungeonRoomAStar>();
        List<DungeonRoomAStar> closedList = new List<DungeonRoomAStar>();
        List<DungeonRoomAStar> adjacentRooms;
        DungeonRoomAStar current = spawnedFloorAStar[start.x, start.y];
        var destinationPos = calculatePosition(destination.x, destination.y);

        // add start node to Open List
        openList.Add(spawnedFloorAStar[start.x, start.y]);

        while (openList.Count != 0 && !closedList.Exists(x => x == spawnedFloorAStar[destination.x, destination.y]))
        {
            current = openList[0];
            openList.Remove(current);
            closedList.Add(current);
            adjacentRooms = getAdjacentNodes(current);

            foreach (var neighbor in adjacentRooms)
            {
                if (!closedList.Contains(neighbor))
                {
                    if (!openList.Contains(neighbor))
                    {
                        neighbor.Parent = current;
                        var neighborPos = calculatePosition(neighbor.GetRoomID().x, neighbor.GetRoomID().y);
                        neighbor.DistanceToTarget = Math.Abs(neighborPos.x - destinationPos.x) + Math.Abs(neighborPos.y - destinationPos.y);
                        if (neighbor.Cost == 1)
                            neighbor.Cost = neighbor.Weight + neighbor.Parent.Cost;
                        openList.Add(neighbor);
                        openList = openList.OrderBy(room => room.AstarValue).ToList();
                    }
                }
            }
        }

        if (returnShortest)
            return null;

        // construct path, if end was not closed return null
        if (!closedList.Exists(x => x == spawnedFloorAStar[destination.x, destination.y]))
            return null;

        // if all good, return path
        DungeonRoomAStar temp = closedList[closedList.IndexOf(current)];
        if (temp == null) return null;

        do
        {
            path.Push(temp);
            temp = temp.Parent;
        } while (temp != spawnedFloorAStar[start.x, start.y] && temp != null);
        return path;

    }

    private List<DungeonRoomAStar> getAdjacentNodes(DungeonRoomAStar room)
    {
        List<DungeonRoomAStar> temp = new List<DungeonRoomAStar>();

        var roomPosX = room.GetRoomID().x;
        var roomPosY = room.GetRoomID().y;

        if (roomPosX - 1 >= 0 && spawnedFloorAStar[roomPosX - 1, roomPosY] != null)
            temp.Add(spawnedFloorAStar[roomPosX - 1, roomPosY]);

        if (roomPosX + 1 <= maxFloorSize.x - 1 && spawnedFloorAStar[roomPosX + 1, roomPosY] != null)
            temp.Add(spawnedFloorAStar[roomPosX + 1, roomPosY]);

        if (roomPosY - 1 >= 0 && spawnedFloorAStar[roomPosX, roomPosY - 1] != null)
            temp.Add(spawnedFloorAStar[roomPosX, roomPosY - 1]);

        if (roomPosY + 1 <= maxFloorSize.y - 1 && spawnedFloorAStar[roomPosX, roomPosY + 1] != null)
            temp.Add(spawnedFloorAStar[roomPosX, roomPosY + 1]);

        return temp;
    }

    Vector2 calculatePosition(int x, int y)
    {
        var xDif = x - startRoomPos.x;
        var yDif = y - startRoomPos.y;

        var xPos = aproxDistanceRoomsX * xDif;
        var yPos = aproxDistanceRoomsY * yDif;

        return new Vector2(xPos, yPos);
    }

    void createFloor()
    {
        //later done by prefab manager

        for (int x = 0; x < maxFloorSize.x; x++)
            for (int y = 0; y < maxFloorSize.y; y++)
                if (floorMap[x, y] != null || floorMap[x, y] != 0)
                    createDungeonRoom(x, y, floorMap[x, y].Value);

    }

    void createDungeonRoom(int x, int y, int roomType)
    {
        GameObject room;

        if (roomType == 9)
            room = Instantiate(startSpawnRoom, calculatePosition(x, y),
                startSpawnRoom.transform.rotation, this.transform);

        room = Instantiate(spawnableRooms[roomType - 1], calculatePosition(x, y),
            spawnableRooms[roomType - 1].transform.rotation, this.transform);

        spawnedFloor[x, y] = room.GetComponent<DungeonRoom>();
        spawnedFloor[x, y].SetRoomID(x, y);
        spawnedFloor[x, y].SetLod(LodActive);
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

        //TODO
        //determine endrooms by Astar and neightbors

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

    public DungeonRoom GetRoomByID(int x, int y)
    {
        if (x >= 0 && y >= 0 && x <= maxFloorSize.x - 1 && y <= maxFloorSize.y - 1)
            return spawnedFloor[x, y];

        return null;
    }

    public Vector2Int GetFloorSize() => maxFloorSize;


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

        debugOutput.text += "\n Astar map";

        for (int i = 0; i < maxFloorSize.x; i++)
        {
            debugOutput.text += "\n";
            for (int j = 0; j < maxFloorSize.y; j++)
            {
                if (spawnedFloorAStar[i, j] == null || floorMap[i, j] == 0)
                    debugOutput.text += " -  ";
                else
                    debugOutput.text += spawnedFloorAStar[i, j].Cost + "  ";
            }
        }
    }


}
