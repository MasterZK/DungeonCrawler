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
    [SerializeField] private int numberOfRooms;
    [SerializeField] private Vector2Int maxFloorSize;
    [SerializeField] private int?[,] floorMap;

    [Header("RoomPrefabs")]
    [SerializeField] private GameObject[] spawnableRooms;
    [SerializeField] private GameObject startSpawnRoom;

    [Header("Debug")]
    [SerializeField] private Text debugOutput;

    private Vector2Int startRoomPos;
    private int spawnedRooms = 0;
    private Stack<Vector2Int> spawnBuffer = new Stack<Vector2Int>();
    private System.Random rand;

    // Start is called before the first frame update
    void Start()
    {
        //spawnableRooms = Resources.LoadAll<GameObject>("RoomPrefabs");

        rand = new System.Random();
        if (useSeed)
            rand = new System.Random(seed);

        do
        {
            spawnFloorMath(maxFloorSize);

            //if (spawnedRooms <= numberOfRooms / 2 && spawnedRooms != 0)
            //    rand = new System.Random(seed / 2);

        } while (spawnedRooms <= numberOfRooms / 2);

        printMapAsText();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void spawnFloorMath(Vector2Int floorSize)
    {
        spawnedRooms = 0;
        floorMap = new int?[floorSize.x, floorSize.y];

        startRoomPos = new Vector2Int(floorSize.x / 2, floorSize.y / 2);
        floorMap[floorSize.x / 2, floorSize.y / 2] = 9;
        spawnedRooms++;

        createSurroundingRooms(startRoomPos.x, startRoomPos.y);
    }

    void createSurroundingRooms(int roomPosX, int roomPosY)
    {
        if (spawnedRooms == numberOfRooms)
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

    void createRoom(int roomPosX, int roomPosY, float roomSpawnChance = 0.5f)
    {
        if (spawnedRooms == numberOfRooms)
            return;

        floorMap[roomPosX, roomPosY] = ChooseRandom(0, rand.Next(1, spawnableRooms.GetLength(0) + 1), roomSpawnChance);

        if (floorMap[roomPosX, roomPosY] != 0)
        {
            spawnedRooms++;
            spawnBuffer.Push(new Vector2Int(roomPosX, roomPosY));
        }
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
