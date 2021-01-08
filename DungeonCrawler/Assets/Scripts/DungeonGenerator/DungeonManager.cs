using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    [SerializeField] private GameObject[] spawnableRooms;
    [SerializeField] private int seed;

    private System.Random rand; 

    // Start is called before the first frame update
    void Start()
    {
        spawnableRooms = Resources.LoadAll<GameObject>("RoomPrefabs");
        
        rand = new System.Random();
        if (seed != 0)
            rand = new System.Random(seed);


    }

    // Update is called once per frame
    void Update()
    {
 
    }
}
