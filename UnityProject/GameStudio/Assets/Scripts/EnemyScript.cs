using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class EnemyScript : MonoBehaviour
{
    //Enums
    public enum MovementStyle {none,patrol};
    public enum AttackPattern {none,line,circle,burst};
    public enum CycleMode {none,linear,random,weightedRandom};

    //PUBLIC VARS
    [Header("Basic Enemy Parameters")]
    public float health;
    public float speed, damageDealt;
    public int scoreValue;
    [Header("Movement")]
    public MovementStyle enemyMovementStyle;
    public Transform[] movementWaypoints;
    [Header("Attacking")]
    public GameObject projectilePrefab;
    public float attackRate;
    public AttackPattern[] attackPatterns;
    [Tooltip("If the Cycle Mode is weighted, these weights determine the frequency one pattern gets selected over another")]
    public float[] attackPatternWeights;
    [Tooltip("If there are multiple attack patterns, this is the random range of how often they will switch attack patterns")]
    public float minimumAttackPatternSwitchTime,maximumAttackPatternSwitchTime;
    [Tooltip("If there are multiple attack patterns, this will determine whether they are selected in a linear order, or selected randomly")]
    public CycleMode attackPatternCycleMode;
    [Header("Spawning")]
    public float spawnInSpeed;
    public float timeUntilSpawn;
    public bool queueSpawnOnStart;
    [Header("Particles")]
    public GameObject attackParticle;
    public GameObject deathParticle;
    [Header("SFX")]
    public AudioClip attackSFX;
    public AudioClip damageSFX,deathSFX;

    //PRIVATE VARS
    [NonSerialized]
    GameObject gm; //gamemanager reference
    AttackPattern selectedAttackPattern;
    Vector3 targetPos;
    int currWaypoint=0;
    bool spawnedIn=false;

    // Start is called before the first frame update
    void Start()
    {
        transform.localScale = Vector3.zero; //Scale is 0,0,0 when not spawned in yet
        if (queueSpawnOnStart) EnemySpawn(timeUntilSpawn);
        if (attackPatterns.Length==0) selectedAttackPattern = AttackPattern.none;
        else selectedAttackPattern = attackPatterns[0]; //Begin with first attack pattern
        if (enemyMovementStyle != MovementStyle.none) targetPos = movementWaypoints[currWaypoint].position;
    }

    // Update is called once per frame
    void Update()
    {
        if (spawnedIn)
        {
            //-----Enemy Movement-----
            switch (enemyMovementStyle)
            {
                case MovementStyle.none: break;
                case MovementStyle.patrol:
                    //Move towards targetPos
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
                    //Check if waypoint is reached
                    float dist = Vector3.Distance(transform.position, targetPos);
                    if (dist < .2) //Arbitrary smallish value to see if position is close enough to targetPos
                    {
                        //Start going to next waypoint
                        currWaypoint++;
                        if (currWaypoint >= movementWaypoints.Length) currWaypoint = 0;
                        targetPos = movementWaypoints[currWaypoint].position;
                    }
                    break;
            }
            //-----Enemy Attacking-----
            //if more 1 attack pattern: switch on the timer based on max and min times;
            switch (selectedAttackPattern)
            {
                case (AttackPattern.none): break;
                case (AttackPattern.line): break;
            }
        }
    }

    public void EnemySpawn(float waitTime=0)
    {
        if (waitTime == 0)
            StartCoroutine(enemySpawnIn());
        else
            StartCoroutine(waitForSpawn());
    }

    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;
        //Play damageSFX
        if (health <= 0) EnemyDie();
    }

    public void EnemyDie()
    {
        //Play death SFX
        Destroy(gameObject);
    }

    IEnumerator enemySpawnIn()
    {
        while (transform.localScale.x < 1)
        {
            transform.localScale += Vector3.one*spawnInSpeed*Time.deltaTime;
            yield return null;
        }
        spawnedIn = true;
    }

    IEnumerator waitForSpawn()
    {
        yield return new WaitForSeconds(timeUntilSpawn);
        EnemySpawn();
    }

    private void OnTriggerEnter(Collider other)
    {
        //collision with player projectiles (account for 3d) -> take damage
    }

}
