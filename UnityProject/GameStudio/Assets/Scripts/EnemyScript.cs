using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    //Enums
    public enum MovementStyle {none,patrol};
    public enum AttackPattern {none,line,circle,burst};
    public enum CycleMode {none,linear,random,weightedRandom};

    //PUBLIC VARS
    [Header("Basic Enemy Parameters")]
    public float health;
    public float speed, damageDealt, projectileSpeed;
    public int scoreValue;
    [Header("Movement")]
    public MovementStyle enemyMovementStyle;
    public Transform[] movementWaypoints;
    [Header("Attacking")]
    public GameObject projectilePrefab;
    public float attackRate;
    public AttackPattern[] attackPatterns;
    [Tooltip("If the Cycle Mode is weighted, these weights determine the frequency one pattern gets selected over another. (All values should add up to 1)")]
    public float[] attackPatternWeights;
    [Tooltip("The number of attacks performed before switching attack patterns (if multiple attack patterns)")]
    public int attacksUntilPatternSwitch;
    [Tooltip("If there are multiple attack patterns, this will determine whether they are selected in a linear order, or selected randomly")]
    public CycleMode attackPatternCycleMode;
    [Header("Spawning")]
    public float spawnInSpeed;
    public float timeUntilSpawnStart;
    public bool queueSpawnOnStart;
    [Tooltip("If false, enemy cannot move and attack while spawning")]
    public bool activeDuringSpawn;
    [Tooltip("Delay time before enemy can move and attack")]
    public float activeDelay;
    public bool canDespawn;
    public float spawnOutSpeed;
    public float timeUntilDespawn;
    [Header("Particles")]
    public GameObject attackParticle;
    public GameObject deathParticle;
    [Header("SFX")]
    public AudioClip attackSFX;
    public AudioClip damageSFX,deathSFX;

    //PRIVATE VARS
    GameObject gm; //gamemanager reference
    [SerializeField]
    GameObject player; //player reference (player, not player controller)
    AttackPattern selectedAttackPattern;
    int currPatternIndex;
    Vector3 targetPos;
    int currWaypoint=0;
    bool spawnedIn=false;
    bool isAttacking = false;
    int atksBeforeSwitchCnt=0;
    int numAttackPatterns;

    // Start is called before the first frame update
    void Start()
    {
        transform.localScale = Vector3.zero; //Scale is 0,0,0 when not spawned in yet
        if (queueSpawnOnStart) EnemySpawn(timeUntilSpawnStart);
        numAttackPatterns = attackPatterns.Length;
        if (numAttackPatterns==0) selectedAttackPattern = AttackPattern.none;
        else selectedAttackPattern = attackPatterns[0]; //Begin with first attack pattern
        currPatternIndex = 0;
        if (enemyMovementStyle != MovementStyle.none) targetPos = movementWaypoints[currWaypoint].position;
        //Sort attackPatternWeights from lowest to highest
        if (attackPatternCycleMode == CycleMode.weightedRandom) Array.Sort(attackPatternWeights,attackPatterns);
        //Start attack timer
        StartCoroutine(enemyAttackTimer());

        //DEBUG
        createProjectile(player.transform.position);
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
            if (isAttacking)
            {
                switch (selectedAttackPattern) //This is where attack is performed (over time possibly)
                {
                    case (AttackPattern.none): break;
                    case (AttackPattern.line):
                        Debug.Log("enemy did LINE attack");
                        isAttacking = false;
                        break;
                    case (AttackPattern.circle):
                        Debug.Log("enemy did CIRCLE attack");
                        isAttacking = false;
                        break;
                    case (AttackPattern.burst):
                        Debug.Log("enemy did BURST attack");
                        isAttacking = false;
                        break;
                }
                if (numAttackPatterns > 1) //If multiple attack patterns
                {
                    if (!isAttacking) atksBeforeSwitchCnt++; //If attack ended, inc counter
                    if (atksBeforeSwitchCnt >= attacksUntilPatternSwitch)
                    {
                        atksBeforeSwitchCnt = 0;
                        switchAttackPattern();
                    }
                }
                if(!isAttacking) StartCoroutine(enemyAttackTimer()); //If attack finished, start timer for next attack
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

    void enemyAttack()
    {
        switch (selectedAttackPattern) //This is where enemy choses and STARTS an attack
        {
            case (AttackPattern.none): break;
            case (AttackPattern.line):
                Debug.Log("enemy started LINE attack");
                break;
            case (AttackPattern.circle):
                Debug.Log("enemy started CIRCLE attack");
                break;
            case (AttackPattern.burst):
                Debug.Log("enemy started BURST attack");
                break;
        }
        //Do attack SFX and attack particle
        isAttacking = true;
    }

    IEnumerator enemyAttackTimer()
    {
        yield return new WaitForSeconds(attackRate);
        enemyAttack();
    }

    public void StartDespawn()
    {
        if (canDespawn)
        {
            StartCoroutine(waitForDespawn());
        }
    }

    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;
        if (health <= 0) EnemyDie();
        //Else
            //Play damageSFX
            //play damage animation
    }

    public void EnemyDie()
    {
        //Play death SFX
        //Instantiate death particle
        Destroy(gameObject);
    }

    void createProjectile(Vector2 _targetPos)
    {
        GameObject bullet = Instantiate(projectilePrefab);
        bullet.AddComponent<EnemyBulletScript>();
        EnemyBulletScript bulletScript = bullet.GetComponent<EnemyBulletScript>();
        //Depth pos ~= to order in layer
        //Calculate velocity using projectileSpeed, and angles using trig and stuff

        //Preparing values for angle calculations
        Vector3 _sPos = transform.position;
        _sPos.z = GetComponent<SpriteRenderer>().sortingOrder;
        int targetDepth = player.GetComponent<SpriteRenderer>().sortingOrder;

        //Re-position startPosition and targetPos so that targetPos=0,0,0 in angle calculations
        Vector3 _pos = new(_sPos.x + ((_targetPos.x>0)?(-_targetPos.x):(_targetPos.x)), _sPos.y + ((_targetPos.y > 0) ? (-_targetPos.y) : (_targetPos.y)),targetDepth-_sPos.z);
        Debug.Log(_pos);
        float xyAngle = Mathf.Atan2(_pos.y,_pos.x) * Mathf.Rad2Deg;
        float xzAngle = Mathf.Atan2(_pos.z,_pos.x) * Mathf.Rad2Deg;
        float yzAngle = Mathf.Atan2(_pos.y,_pos.z) * Mathf.Rad2Deg;
        
        //Angles from the player to the enemy (front-view,topdown-view,side-view)
        Vector3 angleVector = new(xyAngle, xzAngle, yzAngle);
        Debug.Log(angleVector);

        //Set bullet velocity
        bulletScript.velocity = Vector2.zero;
    }

    IEnumerator enemySpawnIn()
    {
        if(activeDuringSpawn) StartCoroutine(enemyActivate());
        while (transform.localScale.x < 1)
        {
            transform.localScale += spawnInSpeed * Time.deltaTime * Vector3.one;
            yield return null;
        }
        transform.localScale = Vector3.one;
        if(!activeDuringSpawn) StartCoroutine(enemyActivate());
        StartDespawn();
    }

    IEnumerator enemyActivate()
    {
        yield return new WaitForSeconds(activeDelay);
        spawnedIn = true;
    }

    IEnumerator waitForSpawn()
    {
        yield return new WaitForSeconds(timeUntilSpawnStart);
        EnemySpawn();
    }

    IEnumerator waitForDespawn()
    {
        yield return new WaitForSeconds(timeUntilDespawn);
        StartCoroutine(enemySpawnOut());
    }

    IEnumerator enemySpawnOut()
    {
        while (transform.localScale.x > 0)
        {
            transform.localScale -= Vector3.one * spawnOutSpeed * Time.deltaTime;
            yield return null;
        }
        transform.localScale = Vector3.zero;
        spawnedIn = false;
        Destroy(gameObject);
    }

    void switchAttackPattern()
    {
        switch (attackPatternCycleMode)
        {
            case (CycleMode.none): break;
            case (CycleMode.linear):
                currPatternIndex++;
                if (currPatternIndex >= numAttackPatterns) currPatternIndex = 0;
                selectedAttackPattern = attackPatterns[currPatternIndex];
                break;
            case (CycleMode.random): //Select random with equal likelihoods
                selectedAttackPattern = attackPatterns[UnityEngine.Random.Range(0, numAttackPatterns)];
                break;
            case (CycleMode.weightedRandom):
                float _rand = UnityEngine.Random.Range(0f, 1f);
                float _prvPercent = 0;
                for(int i=0; i<attackPatternWeights.Length; i++)
                {
                    if (attackPatternWeights[i] + _prvPercent > _rand) {
                        selectedAttackPattern = attackPatterns[i];
                        break;
                    }
                    _prvPercent += attackPatternWeights[i];
                }
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //collision with player projectiles (account for 3d) -> take damage
    }

}
