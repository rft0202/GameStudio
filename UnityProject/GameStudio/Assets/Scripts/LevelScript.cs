using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelScript : MonoBehaviour
{
    [Tooltip("Make Boss the last enemy in list")]
    public GameObject[] enemies;
    [Tooltip("Number of enemies to defeat before enemy[index] spawns")]
    public int[] enemiesToDefeatBeforeSpawn;

    List<bool> enemySpawned = new();


    // Start is called before the first frame update
    void Start()
    {
        for(int i=0; i<enemies.Length; i++)
        {
            enemySpawned.Add(enemies[i].GetComponent<EnemyScript>().queueSpawnOnStart);
        }

    }

    // Update is called once per frame
    void Update()
    {
        int defeatedEnemies=0;
        //Check for new enemy spawning
        for(int i=0; i<enemies.Length; i++)
        {
            if (enemies[i] == null) defeatedEnemies++;
        }

        //Check for spawning new enemy
        for(int i=0; i<enemies.Length; i++)
        {
            //enemies should be not active
            if (enemiesToDefeatBeforeSpawn[i]==defeatedEnemies && !enemySpawned[i])
            {
                enemySpawned[i] = true;
                enemies[i].GetComponent<EnemyScript>().EnemySpawn();
            }
        }
    }
}
