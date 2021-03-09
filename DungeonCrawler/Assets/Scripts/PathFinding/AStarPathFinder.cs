using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class AStarPathFinder : MonoBehaviour
{
    private Queue<AStarSearchCall> requests = new Queue<AStarSearchCall>();
    private AStarSearchCall currentRequest;

    private JobHandle jobHandle;
    private ProcessPathJob job;

    private DungeonGrid grid;

    [BurstCompile]
    private struct ProcessPathJob : IJob
    {
        public DungeonGrid grid;
        public NativeList<DungeonRoomAStar> result;
        public NativeBinaryHeap<DungeonRoomAStar> openNodes;
        public NativeHashMap<ID, DungeonRoomAStar> closedNodes;

        public ID startNode;
        public ID endNode;

        public void Execute()
        {
            var currentNode = grid.floorMap[grid.IDToIndex(startNode)];
            currentNode.setDistanceToTarget(0);
            currentNode.setCost(0);
            grid.floorMap[grid.IDToIndex(startNode)] = currentNode;

            NativeList<DungeonRoomAStar> adjacentRooms = grid.GetAdjacentRooms(currentNode.RoomID);
            var destinationPos = grid.CalculatePosition(endNode);
            openNodes.Add(grid.floorMap[grid.IDToIndex(startNode)]);

            while (openNodes.Count > 0)
            {
                currentNode = openNodes.RemoveFirst();

                if (!closedNodes.TryAdd(currentNode.RoomID, currentNode))
                    break;
                if (currentNode.RoomID == endNode)
                    break;

                adjacentRooms = grid.GetAdjacentRooms(currentNode.RoomID);

                for (int i = 0; i < adjacentRooms.Length; i++)
                {
                    var neighbor = adjacentRooms[i];
                    neighbor.Parent = currentNode.RoomID;

                    if (closedNodes.TryGetValue(neighbor.RoomID, out DungeonRoomAStar _))
                        continue;

                    if (neighbor.Cost == 1)
                        neighbor.Cost = neighbor.Weight + grid.floorMap[grid.IDToIndex(currentNode.RoomID)].Cost;

                    var neighborPos = grid.CalculatePosition(neighbor.RoomID.X, neighbor.RoomID.Y);
                    neighbor.DistanceToTarget = grid.CalculateDistanceToTarget(neighborPos, destinationPos);

                    int neighborIndex = openNodes.IndexOf(neighbor);
                    if (neighborIndex >= 0)
                    {
                        if (neighbor.Cost < openNodes[neighborIndex].Cost)
                        {
                            openNodes.RemoveAt(neighborIndex);
                            openNodes.Add(neighbor);
                        }
                    }
                    else
                    {
                        if (openNodes.Count < openNodes.Capacity)
                            openNodes.Add(neighbor);
                        else
                            return;
                    }

                    grid.floorMap[grid.IDToIndex(neighbor.RoomID)] = neighbor;
                }
            }//while end

            result.Add(grid.floorMap[grid.IDToIndex(endNode)]);
        }//execute end
    }

    public void QueueJob(AStarSearchCall request) => requests.Enqueue(request);

    public void DequeueJob()
    {
        if (requests.Count == 0)
            return;

        currentRequest = requests.Dequeue();

        job = new ProcessPathJob()
        {
            startNode = currentRequest.startNode,
            endNode = currentRequest.endNode,
            grid = grid.Copy(Allocator.TempJob),
            result = new NativeList<DungeonRoomAStar>(1,Allocator.TempJob),
            openNodes = new NativeBinaryHeap<DungeonRoomAStar>((int)(grid.maxFloorSize.x * grid.maxFloorSize.y / 4), Allocator.TempJob),
            closedNodes = new NativeHashMap<ID, DungeonRoomAStar>(128, Allocator.TempJob)
        };
        jobHandle = job.Schedule();

        jobHandle.Complete();
        currentRequest.result = job.result[0];
        currentRequest.IsDone = true;

        job.grid.Dispose();
        job.result.Dispose();
        job.openNodes.Dispose();
        job.closedNodes.Dispose();
        currentRequest = null;
    }

    public void SetGrid(DungeonGrid newGrid)
    {
        grid.Dispose();
        grid = newGrid;
    }

    private void OnDestroy()
    {
        jobHandle.Complete();
        job.grid.Dispose();

        if (job.result.IsCreated)
            job.result.Dispose();

        if (job.openNodes.elements.IsCreated)
            job.openNodes.Dispose();

        if (job.closedNodes.IsCreated)
            job.closedNodes.Dispose();

        this.grid.Dispose();
    }
}

