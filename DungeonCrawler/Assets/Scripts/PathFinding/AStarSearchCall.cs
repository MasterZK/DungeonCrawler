public class AStarSearchCall 
{
    public ID startNode;
    public ID endNode;

    public DungeonRoomAStar result;

    public bool IsDone;
    
    public AStarSearchCall(ID start, ID end)
    {
        startNode = start;
        endNode = end;

        IsDone = false;
    }

    public void Done() => IsDone = true;
}
