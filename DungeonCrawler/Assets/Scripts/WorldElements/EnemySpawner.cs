using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private int currentEnemyCount;
    [SerializeField] private float height, width = 0;

    [Space] [SerializeField] private bool overrideDontSpawn;

    public bool spawned { get; internal set; } = false;

    private EnemyManager enemyManager;
    private List<Actor> spawnedEnemies;
    public ID roomID;

    private void Awake()
    {
        spawnedEnemies = new List<Actor>();
        enemyManager = GameObject.FindObjectOfType<EnemyManager>();
    }

    public void SetSpawnArea(float height, float width)
    {
        this.height = height - 3;
        this.width = width - 3;
    }

    public void EnemyDeath(Actor died)
    {
        currentEnemyCount--;
        spawnedEnemies.Remove(died);
    }

    public void SpawnEnemies()
    {
        if (overrideDontSpawn)
            return;

        spawned = true;

        //TODO change for multiple enemy types
        for (int i = 0; i < enemyManager.GetToSpawnCount(); i++)
        {
            var enemy = Instantiate(enemyManager.MeleeEnemy, 
                (Vector2) this.transform.position + getRandomPosInRange(), 
                Quaternion.identity, this.transform);

            spawnedEnemies.Add(enemy.GetComponent<Actor>());
            spawnedEnemies.Last().SetSpawner(this);
            enemy.GetComponent<EnemyBehaviour>().currentRoom = this.roomID;
        }

        currentEnemyCount = enemyManager.GetToSpawnCount();
    }

    private Vector2 getRandomPosInRange()
    {
        return new Vector2(Random.Range(-width / 2,width / 2), Random.Range(-height / 2, height / 2));
    }

    private void killAll()
    {
        for (int i = 0; i < spawnedEnemies.Count; i++)
            spawnedEnemies[i].kill();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(this.transform.position, new Vector3(width,height));
    }
}
