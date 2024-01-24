using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    //Enums
    public enum MovementStyle {none,patrol};
    public enum AttackPattern {none,single,circle,burst3,line,X,burst5,burst10};
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
                    case (AttackPattern.none): break; //SINGLE TIME ATTACKS--------------
                    case (AttackPattern.single):isAttacking = false;break;
                    case (AttackPattern.circle):isAttacking = false;break;
                    case (AttackPattern.line):isAttacking = false;break;
                    case (AttackPattern.X):isAttacking = false;break;

                    //OVERTIME ATTACKS----------------
                    case (AttackPattern.burst3): 
                        Debug.Log("enemy did BURST3 attack");
                        isAttacking = false;
                        break;
                    case (AttackPattern.burst5):
                        Debug.Log("enemy did BURST5 attack");
                        isAttacking = false;
                        break;
                    case (AttackPattern.burst10):
                        Debug.Log("enemy did BURST10 attack");
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
            case (AttackPattern.none): break; //SINGLE TIME ATTACKS--------------
            case (AttackPattern.single):
                Debug.Log("enemy started SINGLE attack");
                createProjectile(player.transform.position);
                break;
            case (AttackPattern.circle):
                Debug.Log("enemy started CIRCLE attack");
                float circleSize = 3f;
                List<Vector3> circlePattern = new()
                {
                    Vector3.up,Vector3.right,Vector3.left,Vector3.down,
                    new Vector3(.707f,.707f,0), new Vector3(-.707f,.707f,0),
                    new Vector3(.707f,-.707f,0),new Vector3(-.707f,-.707f,0)
                };
                for (int j = 0; j < circlePattern.Count; j++) circlePattern[j] *= circleSize;
                shootBulletFormation(circlePattern);
                break;
            case (AttackPattern.line): break;
            case (AttackPattern.X):
                Debug.Log("enemy started CIRCLE attack");
                float xSize = 1.5f;
                List<Vector3> xPattern = new()
                {
                    new Vector3(1,1,0), new Vector3(-1,1,0),
                    new Vector3(1,-1,0),new Vector3(-1,-1,0),
                    new Vector3(2,2,0), new Vector3(-2,2,0),
                    new Vector3(2,-2,0),new Vector3(-2,-2,0),
                    Vector3.zero,
                };
                for (int j = 0; j < xPattern.Count; j++) xPattern[j] *= xSize;
                shootBulletFormation(xPattern);
                break;

            //OVERTIME ATTACKS----------------
            case (AttackPattern.burst3):
                Debug.Log("enemy started BURST3 attack");
                createProjectile(player.transform.position);
                break;
            case (AttackPattern.burst5):
                Debug.Log("enemy started BURST5 attack");
                createProjectile(player.transform.position);
                break;
            case (AttackPattern.burst10):
                Debug.Log("enemy started BURST10 attack");
                createProjectile(player.transform.position);
                break;
        }
        //Do attack SFX and attack particle
        isAttacking = true;
    }

    void shootBulletFormation(List<Vector3> bulletPositions)
    {
        for(int k=0; k<bulletPositions.Count; k++) createProjectile(player.transform.position + bulletPositions[k]);
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
        //Calculate velocity using projectileSpeed and trig and stuff

        //Preparing values for calculations
        Vector3 _sPos = transform.position;
        _sPos.z = GetComponent<SpriteRenderer>().sortingOrder;
        int targetDepth = player.GetComponent<SpriteRenderer>().sortingOrder;

        //Getting distances on x,y,z
        Vector3 dists = new(_targetPos.x - _sPos.x,_targetPos.y-_sPos.y,targetDepth-_sPos.z);
        //Getting total distance / magnitude
        float totalDist = Mathf.Sqrt(Mathf.Pow(dists.x,2)+ Mathf.Pow(dists.y, 2)+ Mathf.Pow(dists.z, 2));
        //Scaling triangle down so hypotenuse=1, which gives the cos(angle).etc. values
        Vector3 trigVals = dists / totalDist;
        //Velocity = proportional trig values * absolute speed
        bulletScript.velocity = projectileSpeed * trigVals;

        //Making sure projectile is in correct position
        bullet.transform.position = new Vector3(_sPos.x,_sPos.y,0);
        bullet.GetComponent<SpriteRenderer>().sortingOrder = (int)_sPos.z;
        bulletScript.zpos = _sPos.z;
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
