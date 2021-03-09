using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public struct DungeonGrid : IDisposable
{
    public int2 maxFloorSize { get; }
    private int approxDistanceRoomsX;
    private int approxDistanceRoomsY;

    public int2 startRoomPos { get; }
    public NativeArray<DungeonRoomAStar> floorMap;

    public DungeonGrid(int2 maxFloorSize, int seed = 0, bool useSeed = true, int distanceRoomsX = 25, int distanceRoomsY = 15)
    {
        this.maxFloorSize = maxFloorSize;
        approxDistanceRoomsX = distanceRoomsX;
        approxDistanceRoomsY = distanceRoomsY;

        startRoomPos = new int2(maxFloorSize.x / 2, maxFloorSize.y / 2);

        floorMap = new NativeArray<DungeonRoomAStar>(maxFloorSize.x * maxFloorSize.y, Allocator.Persistent);
    }

    public NativeList<DungeonRoomAStar> GetAdjacentRooms(ID roomID)
    {
        NativeList<DungeonRoomAStar> temp = new NativeList<DungeonRoomAStar>(Allocator.Temp);

        var roomPosX = roomID.X;
        var roomPosY = roomID.Y;

        if (roomPosX - 1 >= 0 && floorMap[IDToIndex(new ID(roomPosX - 1, roomPosY))].Null == false)
            temp.Add(floorMap[IDToIndex(new ID(roomPosX - 1, roomPosY))]);

        if (roomPosX + 1 <= maxFloorSize.x - 1 && floorMap[IDToIndex(new ID(roomPosX + 1, roomPosY))].Null == false)
            temp.Add(floorMap[IDToIndex(new ID(roomPosX + 1, roomPosY))]);

        if (roomPosY - 1 >= 0 && floorMap[IDToIndex(new ID(roomPosX, roomPosY - 1))].Null == false)
            temp.Add(floorMap[IDToIndex(new ID(roomPosX, roomPosY - 1))]);

        if (roomPosY + 1 <= maxFloorSize.y - 1 && floorMap[IDToIndex(new ID(roomPosX, roomPosY + 1))].Null == false)
            temp.Add(floorMap[IDToIndex(new ID(roomPosX, roomPosY + 1))]);

        return temp;
    }

    public Vector2 CalculatePosition(int x, int y)
    {
        var xDif = x - startRoomPos.x;
        var yDif = y - startRoomPos.y;

        var xPos = approxDistanceRoomsX * xDif;
        var yPos = approxDistanceRoomsY * yDif;

        return new Vector2(xPos, yPos);
    }

    public Vector2 CalculatePosition(ID roomID) => CalculatePosition(roomID.X, roomID.Y);

    public float CalculateDistanceToTarget(Vector2 start, Vector2 target) => math.abs(start.x - target.x) + math.abs(start.y - target.y);

    public ID IndexToID(int index)
    {
        int column = index % maxFloorSize.y;
        int row = (index - column) / maxFloorSize.y;

        return new ID(row, column);
    }

    public int IDToIndex(int x, int y)
    {
        return (x * maxFloorSize.y) + y;
    }

    public int IDToIndex(ID roomID) => IDToIndex(roomID.X, roomID.Y);

    public DungeonGrid Copy(Allocator allocator = Allocator.TempJob)
    {
        DungeonGrid newGrid = this;
        newGrid.floorMap = new NativeArray<DungeonRoomAStar>(floorMap.Length, allocator);
        //this.floorMap.CopyTo(newGrid.floorMap);
        newGrid.floorMap.CopyFrom(floorMap);

        return newGrid;
    }

    public void Dispose()
    {
        if (floorMap.IsCreated)
            floorMap.Dispose();
    }
}