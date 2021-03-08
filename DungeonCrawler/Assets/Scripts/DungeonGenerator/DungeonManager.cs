using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
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

    private int spawnedRooms = 0;
    private Stack<Vector2Int> spawnBuffer = new Stack<Vector2Int>();
    private System.Random rand;

    private List<DungeonRoomAStar> spawneDungeonRoomAStars;
    [ItemCanBeNull] private DungeonRoom[,] spawnedFloor;

    void Start()
    {
        float startTime = Time.realtimeSinceStartup;

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

        //if (createAstar)
        //    createAstarFloorJob();

        if (createRooms)
        {
            createFloor();

            foreach (var room in spawnedFloor)
                if (room != null)
                    room.setTeleporters();
        }

        Debug.Log("Time: " + ((Time.realtimeSinceStartup - startTime) * 1000f));

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

        var startID = dungeonFloor.startRoomPos;
        dungeonFloor.floorMap[dungeonFloor.IDToIndex(startID)] = new DungeonRoomAStar(startID, false);
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
        dungeonFloor.floorMap[index] = CreateRoom(roomPosX, roomPosY, floorDensity);

        if (!dungeonFloor.floorMap[index].Null && dungeonFloor.floorMap[index].created)
        {
            spawnBuffer.Push(new Vector2Int(roomPosX, roomPosY));
            spawnedRooms++;
        }
    }

    public DungeonRoomAStar CreateRoom(ID roomID, float chance)
    {
        var result = ChooseRandom(0, 1, chance);
        Debug.Log(result);


        return new DungeonRoomAStar(roomID, result != 1);
    }

    public DungeonRoomAStar CreateRoom(int x, int y, float chance) => CreateRoom(new ID(x, y), chance);

    public int ChooseRandom(int valueOne, int valueTwo, float chance)
    {
        chance *= 10;
        if (rand.Next(1, 11) > chance)
            return valueOne;

        return valueTwo;
    }

    /*
    void createAstarFloorJob()
    {
        spawneDungeonRoomAStars = new List<DungeonRoomAStar>(spawnedRooms);
        NativeHashMap<ID, DungeonRoomAStar> aStarHashMap = new NativeHashMap<ID, DungeonRoomAStar>(spawnedRooms, Allocator.Persistent);

        for (int index = 0; index < maxFloorSize.x * maxFloorSize.y; index++)
        {
            var id = IndexToID(index);

            if (floorMap[id.X, id.Y] != null && floorMap[id.X, id.Y] != 0)
            {
                var room = new DungeonRoomAStar(id, false);
                spawneDungeonRoomAStars.Add(room);
                aStarHashMap.Add(id, room);
            }
        }

        NativeList<JobHandle> calAstarJobs = new NativeList<JobHandle>(spawnedRooms, Allocator.Persistent);

        for (int index = 0; index < spawneDungeonRoomAStars.Count; index++)
        {
            var id = spawneDungeonRoomAStars[index].RoomID;

            if (id == (ID)startRoomPos)
                return;

            CalAStarJob aStarJob = new CalAStarJob
            {
                floorSize = maxFloorSize,
                aproxDistanceX = this.approxDistanceRoomsX,
                aproxDistanceY = this.approxDistanceRoomsY,
                startPosition = startRoomPos,
                endPosition = id,
                dungeonStars = aStarHashMap,
            };

            if (calAstarJobs.Length != 0)
            {
                calAstarJobs.Add(aStarJob.Schedule(calAstarJobs[calAstarJobs.Length - 1]));
                continue;
            }

            calAstarJobs.Add(aStarJob.Schedule());
        }

        JobHandle.CompleteAll(calAstarJobs);

        var result = aStarHashMap.GetValueArray(Allocator.Temp);
        spawneDungeonRoomAStars.Clear();
        spawneDungeonRoomAStars.AddRange(result.ToList());

        aStarHashMap.Dispose();
        result.Dispose();
        calAstarJobs.Dispose();
    }

    void createAstarFloorJobParallelFor()
    {
        DungeonRoomAStar[] aStarFloor = new DungeonRoomAStar[maxFloorSize.x * maxFloorSize.y];

        for (int index = 0; index < maxFloorSize.x * maxFloorSize.y; index++)
        {
            var id = IndexToID(index);

            if (floorMap[id.X, id.Y] != null && floorMap[id.X, id.Y] != 0)
            {
                aStarFloor[index] = new DungeonRoomAStar(id, false);
                continue;
            }

            aStarFloor[index] = new DungeonRoomAStar(id);
        }

        NativeArray<DungeonRoomAStar> aStarRooms = new NativeArray<DungeonRoomAStar>(maxFloorSize.x * maxFloorSize.y, Allocator.TempJob);
        aStarRooms.CopyFrom(aStarFloor);

        CalAStarJobParallel aStarCalParallel = new CalAStarJobParallel
        {
            floorSize = maxFloorSize,
            aproxDistanceX = this.approxDistanceRoomsX,
            aproxDistanceY = this.approxDistanceRoomsY,
            startPosition = startRoomPos,
            dungeonStars = aStarRooms,
        };

        JobHandle aStarJob = aStarCalParallel.Schedule(maxFloorSize.x * maxFloorSize.y, spawnedRooms / SystemInfo.processorCount);
        aStarJob.Complete();

        spawneDungeonRoomAStars = new List<DungeonRoomAStar>(spawnedRooms);
        spawneDungeonRoomAStars = aStarFloor.Where(x => x.Null == false).ToList();
        aStarRooms.Dispose();
    }*/

    public ID IndexToID(int index)
    {
        int column = index % maxFloorSize.y;
        int row = (index - column) / maxFloorSize.y;

        return new ID(row, column);
    }

    public int IDToIndex(int x, int y)
    {
        return x * maxFloorSize.y + y;
    }

    private struct CalAStarJobParallel : IJobParallelFor
    {
        public int2 floorSize;
        public int aproxDistanceX;
        public int aproxDistanceY;

        public ID startPosition;
        public ID endPosition;

        [NativeDisableParallelForRestriction]
        public NativeArray<DungeonRoomAStar> dungeonStars;

        public void Execute(int index)
        {
            if (dungeonStars[index].Null == true)
                return;

            NativeList<DungeonRoomAStar> openList = new NativeList<DungeonRoomAStar>(Allocator.Temp);
            NativeList<DungeonRoomAStar> closedList = new NativeList<DungeonRoomAStar>(Allocator.Temp);

            endPosition = IndexToID(index);
            dungeonStars[IDtoIndex(startPosition)].setDistanceToTarget(0);
            dungeonStars[IDtoIndex(startPosition)].setCost(0);

            DungeonRoomAStar current = dungeonStars[IDtoIndex(new ID(startPosition.X, startPosition.Y))];
            NativeList<DungeonRoomAStar> adjacentRooms = getAdjacentRooms(current.RoomID);
            var destinationPos = calculatePosition(endPosition.X, endPosition.Y);

            // add start node to Open List
            openList.Add(dungeonStars[IDtoIndex(startPosition)]);

            while (openList.Length != 0 && !closedList.Contains(dungeonStars[IDtoIndex(endPosition)]))
            {
                current = openList[0];
                openList.RemoveAt(openList.IndexOf(current));
                closedList.Add(current);
                adjacentRooms = getAdjacentRooms(current.RoomID);

                for (int i = 0; i < adjacentRooms.Length; i++)
                {
                    var neighbor = adjacentRooms[i];

                    if (!closedList.Contains(neighbor))
                    {
                        if (!openList.Contains(neighbor))
                        {
                            neighbor.Parent = current.RoomID;
                            var neighborPos = calculatePosition(neighbor.RoomID.X, neighbor.RoomID.Y);
                            neighbor.DistanceToTarget = calculateDistanceToTarget(neighborPos, destinationPos);
                            if (neighbor.Cost == 1)
                                neighbor.Cost = neighbor.Weight + dungeonStars[IDtoIndex(neighbor.Parent)].Cost;
                            Debug.Log(neighbor.Cost);

                            openList.Add(neighbor);
                            openList.Sort(new OrderByComparer());
                        }
                    }
                }
            }

            //Debug.Log("Time: " + ((Time.realtimeSinceStartup - startTime) * 1000f));

            adjacentRooms.Dispose();
            closedList.Dispose();
            openList.Dispose();
        }

        private struct OrderByComparer : IComparer<DungeonRoomAStar>
        {
            public int Compare(DungeonRoomAStar x, DungeonRoomAStar y)
            {
                if (x.AstarValue == y.AstarValue)
                    return 0;

                if (x.AstarValue > y.AstarValue)
                    return 1;

                return -1;
            }
        }

        private NativeList<DungeonRoomAStar> getAdjacentRooms(ID roomID)
        {
            NativeList<DungeonRoomAStar> temp = new NativeList<DungeonRoomAStar>(Allocator.TempJob);

            var roomPosX = roomID.X;
            var roomPosY = roomID.Y;

            if (roomPosX - 1 >= 0 && dungeonStars[IDtoIndex(new ID(roomPosX - 1, roomPosY))].Null == false)
                temp.Add(dungeonStars[IDtoIndex(new ID(roomPosX - 1, roomPosY))]);

            if (roomPosX + 1 <= floorSize.x - 1 && dungeonStars[IDtoIndex(new ID(roomPosX + 1, roomPosY))].Null == false)
                temp.Add(dungeonStars[IDtoIndex(new ID(roomPosX + 1, roomPosY))]);

            if (roomPosY - 1 >= 0 && dungeonStars[IDtoIndex(new ID(roomPosX, roomPosY - 1))].Null == false)
                temp.Add(dungeonStars[IDtoIndex(new ID(roomPosX, roomPosY - 1))]);

            if (roomPosY + 1 <= floorSize.y - 1 && dungeonStars[IDtoIndex(new ID(roomPosX, roomPosY + 1))].Null == false)
                temp.Add(dungeonStars[IDtoIndex(new ID(roomPosX, roomPosY + 1))]);

            return temp;
        }

        Vector2 calculatePosition(int x, int y)
        {
            var xDif = x - startPosition.X;
            var yDif = y - startPosition.Y;

            var xPos = aproxDistanceX * xDif;
            var yPos = aproxDistanceY * yDif;

            return new Vector2(xPos, yPos);
        }

        private float calculateDistanceToTarget(Vector2 start, Vector2 target) => math.abs(start.x - target.x) + math.abs(start.y - target.y);

        private ID IndexToID(int index)
        {
            int column = index % floorSize.y;
            int row = (index - column) / floorSize.y;

            return new ID(row, column);
        }

        private int IDtoIndex(int x, int y)
        {
            return x * floorSize.y + y;
        }

        private int IDtoIndex(ID id)
        {
            return id.X * floorSize.y + id.Y;
        }
    }

    private struct CalAStarJob : IJob
    {
        public int2 floorSize;
        public int aproxDistanceX;
        public int aproxDistanceY;

        public ID startPosition;
        public ID endPosition;

        public NativeHashMap<ID, DungeonRoomAStar> dungeonStars;

        public void Execute()
        {
            NativeList<DungeonRoomAStar> openList = new NativeList<DungeonRoomAStar>(Allocator.Temp);
            NativeList<DungeonRoomAStar> closedList = new NativeList<DungeonRoomAStar>(Allocator.Temp);
            DungeonRoomAStar current = dungeonStars[new ID(startPosition.X, startPosition.Y)];
            NativeList<DungeonRoomAStar> adjacentRooms = getAdjacentRooms(current.RoomID);
            var destinationPos = calculatePosition(endPosition.X, endPosition.Y);

            // add start node to Open List
            openList.Add(dungeonStars[new ID(startPosition.X, startPosition.Y)]);

            while (openList.Length != 0 && !closedList.Contains(dungeonStars[endPosition]))
            {
                current = openList[0];
                openList.RemoveAt(openList.IndexOf(current));
                closedList.Add(current);
                adjacentRooms = getAdjacentRooms(current.RoomID);

                for (int i = 0; i < adjacentRooms.Length; i++)
                {
                    var neighbor = adjacentRooms[i];

                    if (!closedList.Contains(neighbor))
                    {
                        if (!openList.Contains(neighbor))
                        {
                            neighbor.Parent = current.RoomID;
                            var neighborPos = calculatePosition(neighbor.RoomID.X, neighbor.RoomID.Y);
                            neighbor.DistanceToTarget = math.abs(neighborPos.x - destinationPos.x) + math.abs(neighborPos.y - destinationPos.y);
                            if (neighbor.Cost == 1)
                                neighbor.Cost = neighbor.Weight + dungeonStars[neighbor.Parent].Cost;

                            Debug.Log(neighbor.Cost);

                            openList.Add(neighbor);
                            openList.Sort(new OrderByComparer());
                        }
                    }
                }
            }

            //Debug.Log("Time: " + ((Time.realtimeSinceStartup - startTime) * 1000f));

            adjacentRooms.Dispose();
            closedList.Dispose();
            openList.Dispose();
        }

        private struct OrderByComparer : IComparer<DungeonRoomAStar>
        {
            public int Compare(DungeonRoomAStar x, DungeonRoomAStar y)
            {
                if (x.AstarValue > y.AstarValue)
                    return 1;

                return -1;
            }
        }

        private NativeList<DungeonRoomAStar> getAdjacentRooms(ID roomID)
        {
            NativeList<DungeonRoomAStar> temp = new NativeList<DungeonRoomAStar>(Allocator.Temp);

            var roomPosX = roomID.X;
            var roomPosY = roomID.Y;

            if (roomPosX - 1 >= 0 && dungeonStars.ContainsKey(new ID(roomPosX - 1, roomPosY)))
                temp.Add(dungeonStars[new ID(roomPosX - 1, roomPosY)]);

            if (roomPosX + 1 <= floorSize.x - 1 && dungeonStars.ContainsKey(new ID(roomPosX + 1, roomPosY)))
                temp.Add(dungeonStars[new ID(roomPosX + 1, roomPosY)]);

            if (roomPosY - 1 >= 0 && dungeonStars.ContainsKey(new ID(roomPosX, roomPosY - 1)))
                temp.Add(dungeonStars[new ID(roomPosX, roomPosY - 1)]);

            if (roomPosY + 1 <= floorSize.y - 1 && dungeonStars.ContainsKey(new ID(roomPosX, roomPosY + 1)))
                temp.Add(dungeonStars[new ID(roomPosX, roomPosY + 1)]);

            return temp;
        }

        Vector2 calculatePosition(int x, int y)
        {
            var xDif = x - startPosition.X;
            var yDif = y - startPosition.Y;

            var xPos = aproxDistanceX * xDif;
            var yPos = aproxDistanceY * yDif;

            return new Vector2(xPos, yPos);
        }

        private ID IndexToID(int index)
        {
            int column = index % floorSize.y;
            int row = (index - column) / floorSize.y;

            return new ID(row, column);
        }

        private int IDtoIndex(int x, int y)
        {
            return x * floorSize.y + y;
        }

        private int IDtoIndex(ID id)
        {
            return id.X * floorSize.y + id.Y;
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
        spawnedFloor = new DungeonRoom[maxFloorSize.x, maxFloorSize.y];

        for (int x = 0; x < maxFloorSize.x; x++)
            for (int y = 0; y < maxFloorSize.y; y++)
            {
                var room = dungeonFloor.floorMap[dungeonFloor.IDToIndex(x, y)];

                if (room.created && !room.Null)
                    createDungeonRoom(x, y, 1);
            }
    }

    void createDungeonRoom(int x, int y, int roomType)
    {
        GameObject room;

        if (roomType == 9)
            room = Instantiate(startSpawnRoom, calculatePosition(x, y),
                startSpawnRoom.transform.rotation, this.transform);
        else
            room = Instantiate(spawnableRooms[roomType - 1], calculatePosition(x, y),
                spawnableRooms[roomType - 1].transform.rotation, this.transform);

        spawnedFloor[x, y] = room.GetComponent<DungeonRoom>();
        spawnedFloor[x, y].SetRoomID(x, y);
        spawnedFloor[x, y].SetLod(LodActive);

        if (roomType != 9)
            spawnedFloor[x, y].gameObject.SetActive(!debugSetActive);
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

    public DungeonRoom GetRoomByID(ID roomID)
    {
        return GetRoomByID(roomID.X, roomID.Y);
    }

    public int2 GetFloorSize() => maxFloorSize;

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
