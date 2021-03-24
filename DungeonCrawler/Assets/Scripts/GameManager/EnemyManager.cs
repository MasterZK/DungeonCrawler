using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("Spawner Settings")]
    [SerializeField] private int enemysPerRoom;

    [Header("Enemy Prefabs")]
    [SerializeField] public GameObject MeleeEnemy;

    public int GetToSpawnCount() => enemysPerRoom;

}
