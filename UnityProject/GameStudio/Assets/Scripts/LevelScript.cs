using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelScript : MonoBehaviour
{
    [Tooltip("Make Boss the last enemy in list")]
    public GameObject[] enemies;
    [Tooltip("Number of enemies to defeat before enemy[index] spawns")]
    public int[] enemiesToDefeatBeforeSpawn;
    [Tooltip("Rate at which clouds spawn, in seconds")]
    public float cloudSpawnRate;

    public GameObject bossSpawnWarning;
    public GameObject cloudPrefab;
    Animator warningAnim;

    List<bool> enemySpawned = new();
    List<GameObject> clouds = new();

    SoundManager sm;


    // Start is called before the first frame update
    void Start()
    {
        for(int i=0; i<enemies.Length; i++)
        {
            enemySpawned.Add(enemies[i].GetComponent<EnemyScript>().queueSpawnOnStart);
        }
        warningAnim = bossSpawnWarning.GetComponent<Animator>();
        sm = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        StartCoroutine(spawnCloud());
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
                if (i == enemies.Length - 1)
                    StartCoroutine(bossWarning());
                else
                    enemies[i].GetComponent<EnemyScript>().EnemySpawn();
            }
        }
    }

    IEnumerator bossWarning()
    {
        warningAnim.SetTrigger("enter");
        sm.PlaySFX(9);
        yield return new WaitForSeconds(2);
        warningAnim.SetTrigger("exit");
        enemies[^1].GetComponent<EnemyScript>().EnemySpawn();

    }

    IEnumerator spawnCloud()
    {
        yield return new WaitForSeconds(cloudSpawnRate);
        clouds.Add(Instantiate(cloudPrefab, new Vector3(Random.Range(-8, 8), Random.Range(-4, 4), 0), Quaternion.identity));
        StartCoroutine(destroyCloud(clouds[^1]));
        StartCoroutine(spawnCloud());
    }

    IEnumerator destroyCloud(GameObject _c)
    {
        yield return new WaitForSeconds(1.5f);
        clouds.Remove(_c);
        Destroy(_c);
    }
}
