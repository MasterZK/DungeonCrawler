using System;
using Unity.Mathematics;

public struct DungeonRoomAStar : IEquatable<DungeonRoomAStar>, IComparable<DungeonRoomAStar>
{
    public ID Parent;
    public int Weight { get; }

    public int Cost; //gCost

    public void setCost(int cost) => this.Cost = cost;

    public float DistanceToTarget; //hCost

    public void setDistanceToTarget(float distance) => this.DistanceToTarget = distance;

    public float AstarValue //fCost
    {
        get
        {
            if (DistanceToTarget != -1 && Cost != -1)
                return DistanceToTarget + Cost;

            return -1;
        }
    }

    public ID RoomID { get; }
    public bool Null { get; } //walkable
    public bool created;

    public DungeonRoomAStar(int2 roomID, bool empty = true, int weight = 1)
    {
        Parent = int2.zero;
        DistanceToTarget = -1;
        Cost = 1;

        Weight = weight;
        RoomID = roomID;
        Null = empty;
        created = true;
    }

    public bool Equals(DungeonRoomAStar other) => this == other;

    public static bool operator ==(DungeonRoomAStar valueOne, DungeonRoomAStar valueTwo) => valueOne.RoomID == valueTwo.RoomID;

    public static bool operator !=(DungeonRoomAStar valueOne, DungeonRoomAStar valueTwo) => valueOne.RoomID != valueTwo.RoomID;

    public override bool Equals(object obj) => (obj is DungeonRoomAStar room) && Equals(room);

    public override int GetHashCode() => (AstarValue).GetHashCode();

    public int CompareTo(DungeonRoomAStar other)
    {
        int compare = AstarValue.CompareTo(other.AstarValue);

        if (compare == 0)
            compare = DistanceToTarget.CompareTo(other.DistanceToTarget);
       
        return -compare;
    }
}