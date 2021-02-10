using UnityEngine;

public class DungeonRoomAStar 
{
    [Header("A-star values")]
    public DungeonRoomAStar Parent;
    public float DistanceToTarget;
    public int Weight;
    public int Cost;
    public float AstarValue //determines the effectiveness of path
    {
        get
        {
            if (DistanceToTarget != -1 && Cost != -1)
                return DistanceToTarget + Cost;

            return -1;
        }
    }

    public DungeonRoomAStar(int weight = 1)
    {
        DistanceToTarget = -1;
        Cost = 1;
        Weight = weight;
    }

    private ID roomID;

    public void SetRoomID(int xID, int yID) => roomID = new ID(xID, yID);
    
    public ID GetRoomID() => roomID;
}
