using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyScript : MonoBehaviour
{
    //Enums
    public enum MovementStyle {none,patrol};
    public enum AttackPattern {none,single,circle,line,X,burst,burstCircle,burstLine,burstX};
    public enum CycleMode {none,linear,random,weightedRandom};
    public enum AttackTarget {player,center,topleft,top,topright,centerleft,centerright,bottomleft,bottom,bottomright};

    //PUBLIC VARS
    [Header("----------Basic Enemy Parameters----------")]
    [Space(4)]
    public float health;
    public float speed, projectileSpeed;
    public int damageDealt;
    public int scoreValue;
    [Tooltip("If true, killing this enemy ends level.")]
    public bool isBoss;

    [Space(8)]
    [Header("----------Movement----------")]
    [Space(4)]
    public MovementStyle enemyMovementStyle;
    public Transform[] movementWaypoints;

    [Space(8)]
    [Header("----------Attacking----------")]
    [Space(4)]
    public GameObject projectilePrefab;
    public bool homingBullets;
    public float attackRate;
    public AttackPattern[] attackPatterns;
    [Tooltip("This array should line up with the attack patterns array.")]
    public AttackTarget[] attackTargets;
    [Tooltip("If the Cycle Mode is weighted, these weights determine the frequency one pattern gets selected over another. (All values should add up to 1)")]
    public float[] attackPatternWeights;
    [Tooltip("The number of attacks performed before switching attack patterns (if multiple attack patterns)")]
    public int attacksUntilPatternSwitch;
    [Tooltip("If there are multiple attack patterns, this will determine whether they are selected in a linear order, or selected randomly")]
    public CycleMode attackPatternCycleMode;

    [Space(8)]
    [Header("----------Attack Pattern Settings----------")]
    [Space(4)]
    [Tooltip("Line angle in radians (pi=180)")]
    [Range(0f, Mathf.PI)]
    public float lineAngle;
    public float lineLength;
    [Range(3, 15)]
    public int lineBulletAmount;
    public float circleSize,xSize;
    [Range(2, 15)]
    public int burstBulletAmount;
    public float burstFireRate;
    //public bool burstsLockTarget;

    [Space(8)]
    [Header("----------Spawning----------")]
    [Space(4)]
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

    [Space(8)]
    [Header("----------Particles----------")]
    [Space(4)]
    public GameObject attackParticle;
    public GameObject deathParticle;

    [Space(8)]
    [Header("----------SFX----------")]
    [Space(4)]
    [Tooltip("ID for sfx, or index of sfx in the SoundManager sfxs array")]
    public int attackSFX;
    [Tooltip("ID for sfx, or index of sfx in the SoundManager sfxs array")]
    public int damageSFX,deathSFX;

    //PRIVATE VARS
    GameManager gm; //gamemanager reference
    [SerializeField]
    GameObject player; //player reference (player, not player controller)
    AttackPattern selectedAttackPattern;
    AttackTarget selectedAttackTarget;
    int currPatternIndex;
    Vector3 targetPos;
    int currWaypoint=0;
    bool spawnedIn=false,dying=false;
    int atksBeforeSwitchCnt=0;
    int numAttackPatterns;
    ScaleBasedOnDepth scaleDepth;
    int burstFireCnt=0;
    Vector3 burstTarget;

    //Animator
    Animator anim;

    //SoundManager
    SoundManager sm;

    MenuScript menuScript;

    // Start is called before the first frame update
    void Start()
    {
        sm = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        anim = GetComponent<Animator>();
        transform.localScale = Vector3.zero; //Scale is 0,0,0 when not spawned in yet
        scaleDepth = GetComponent<ScaleBasedOnDepth>();
        scaleDepth.enabled = false;
        if (queueSpawnOnStart) EnemySpawn(timeUntilSpawnStart);
        numAttackPatterns = attackPatterns.Length;
        if (numAttackPatterns == 0) selectedAttackPattern = AttackPattern.none;
        else { selectedAttackPattern = attackPatterns[0]; selectedAttackTarget = attackTargets[0]; } //Begin with first attack pattern
        currPatternIndex = 0;
        if (enemyMovementStyle != MovementStyle.none) targetPos = movementWaypoints[currWaypoint].position;
        //Sort attackPatternWeights from lowest to highest
        if (attackPatternCycleMode == CycleMode.weightedRandom) Array.Sort(attackPatternWeights,attackPatterns);
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
        if (spawnedIn)
        {
            if (selectedAttackPattern != AttackPattern.none)
                sm.PlaySFX(attackSFX, UnityEngine.Random.Range(0.9f, 1.15f));

            switch (selectedAttackPattern) //This is where enemy choses and STARTS an attack
            {
                case (AttackPattern.none): break; //SINGLE TIME ATTACKS--------------
                case (AttackPattern.single):
                    createProjectile(getTargetPos());
                    break;
                case (AttackPattern.circle):
                    List<Vector3> circlePattern = new()
                {
                    Vector3.up,Vector3.right,Vector3.left,Vector3.down,
                    new Vector3(.707f,.707f,0), new Vector3(-.707f,.707f,0),
                    new Vector3(.707f,-.707f,0),new Vector3(-.707f,-.707f,0)
                };
                    for (int j = 0; j < circlePattern.Count; j++) circlePattern[j] *= circleSize;
                    shootBulletFormation(circlePattern);
                    break;
                case (AttackPattern.line):
                    shootBulletFormation(getLinePattern());
                    break;
                case (AttackPattern.X):
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

                //BURST ATTACKS----------------
                case (AttackPattern.burst):
                    burstFireCnt = 0;
                    burstTarget = getTargetPos();
                    StartCoroutine(burstAttack(new List<Vector3>()));
                    break;
                case (AttackPattern.burstCircle):
                    List<Vector3> circlePattern1 = new()
                {
                    Vector3.up,Vector3.right,Vector3.left,Vector3.down,
                    new Vector3(.707f,.707f,0), new Vector3(-.707f,.707f,0),
                    new Vector3(.707f,-.707f,0),new Vector3(-.707f,-.707f,0)
                };
                    for (int j = 0; j < circlePattern1.Count; j++) circlePattern1[j] *= circleSize;
                    burstFireCnt = 0;
                    burstTarget = getTargetPos();
                    StartCoroutine(burstAttack(circlePattern1));
                    break;
                case (AttackPattern.burstLine):
                    burstFireCnt = 0;
                    burstTarget = getTargetPos();
                    StartCoroutine(burstAttack(getLinePattern()));
                    break;
                case (AttackPattern.burstX):
                    List<Vector3> xPattern1 = new()
                {
                    new Vector3(1,1,0), new Vector3(-1,1,0),
                    new Vector3(1,-1,0),new Vector3(-1,-1,0),
                    new Vector3(2,2,0), new Vector3(-2,2,0),
                    new Vector3(2,-2,0),new Vector3(-2,-2,0),
                    Vector3.zero,
                };
                    for (int j = 0; j < xPattern1.Count; j++) xPattern1[j] *= xSize;
                    burstFireCnt = 0;
                    burstTarget = getTargetPos();
                    StartCoroutine(burstAttack(xPattern1));
                    break;
            }
            //Do attack SFX and attack particle
            if (numAttackPatterns > 1) //If multiple attack patterns
            {
                atksBeforeSwitchCnt++; //Counter for switching attack patterns
                if (atksBeforeSwitchCnt >= attacksUntilPatternSwitch)
                {
                    atksBeforeSwitchCnt = 0;
                    switchAttackPattern();
                }
            }
            StartCoroutine(enemyAttackTimer());
        }
    }

    float getLineX()
    {
        float _cos2a = Mathf.Pow(Mathf.Cos(lineAngle), 2), _sin2a = Mathf.Pow(Mathf.Sin(lineAngle), 2);
        return Mathf.Sqrt(Mathf.Pow(lineLength, 2) * _cos2a / (_cos2a + _sin2a));
    }
    float getLineY(float _x)
    {
        return (Mathf.Sin(lineAngle) * _x / Mathf.Cos(lineAngle));
    }
    List<Vector3> getLinePattern()
    {
        List<Vector3> _linePattern = new();
        float lineMax = getLineX(), lineMin=-lineMax;
        _linePattern.Add(new Vector3(lineMin, getLineY(lineMin), 0));
        _linePattern.Add(new Vector3(lineMax, getLineY(lineMax), 0));
        int lnCnt=lineBulletAmount-2;
        float bulletIncX = lineMax-(lineMin / ((((float)lnCnt - 1) / 2) + 1) +lineMax);
        float _lx=lineMin+bulletIncX;
        for(int m=0; m<lnCnt; m++)
        {
            _linePattern.Add(new Vector3(_lx, getLineY(_lx), 0));
            _lx += bulletIncX;
        }

        return _linePattern;
    }

    IEnumerator burstAttack(List<Vector3> _bulletPositions)
    {
        //If burstsLockTarget, keep targeting the same pos, else continue updating to player's new pos
        //if (!burstsLockTarget) burstTarget = getTargetPos();
        //If multiple bullets, shoot formation, otherwise just shoot one bullet
        if (_bulletPositions.Count > 1) shootBulletFormation(_bulletPositions,burstTarget);
        else createProjectile(burstTarget);

        burstFireCnt++; //If the burst isn't complete, wait and repeat, else stop
        if (burstFireCnt < burstBulletAmount)
        {
            yield return new WaitForSeconds(burstFireRate);
            StartCoroutine(burstAttack(_bulletPositions));
        }
    }

    void shootBulletFormation(List<Vector3> bulletPositions)
    {
        for(int k=0; k<bulletPositions.Count; k++) createProjectile(getTargetPos() + bulletPositions[k]);
    }

    void shootBulletFormation(List<Vector3> bulletPositions, Vector3 _target)
    {
        for (int k = 0; k < bulletPositions.Count; k++) createProjectile(_target + bulletPositions[k]);
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
        if (health <= 0 && !dying) EnemyDie();
        //Else
        anim.SetTrigger("damaged");
            //Play damageSFX
            //play damage animation
    }

    public void EnemyDie()
    {
        //Play death SFX
        //Instantiate death particle

        gm.AddToActiveScore(scoreValue,new Vector2(transform.position.x+1,transform.position.y+1));
        gm.enemiesToKill--;
        if (isBoss)
        {
            dying = true;
            spawnedIn = false;
            GetComponent<Renderer>().enabled = false;
            StartCoroutine(levelEnd(1f));
        }
        else Destroy(gameObject);
    }

    IEnumerator levelEnd(float delay)
    {
        yield return new WaitForSeconds(delay);
        Cursor.visible = true;
        SceneManager.LoadScene("LevelComplete");
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
        bullet.GetComponent<ScaleBasedOnDepth>().zpos = _sPos.z;

        //Projectile inherits damage from enemy
        bulletScript.damage = damageDealt;

        //If homing bullet
        if (homingBullets)
        {
            bulletScript.homingBullet = true;
            bulletScript.speed = projectileSpeed;
        }
    }

    IEnumerator enemySpawnIn()
    {
        if(activeDuringSpawn) StartCoroutine(enemyActivate());
        while (transform.localScale.x < scaleDepth.GetTargetScale().x)
        {
            transform.localScale += spawnInSpeed * Time.deltaTime * Vector3.one;
            yield return null;
        }
        transform.localScale = Vector3.one;
        scaleDepth.enabled = true;
        if (!activeDuringSpawn) StartCoroutine(enemyActivate());
        StartDespawn();
    }

    IEnumerator enemyActivate()
    {
        yield return new WaitForSeconds(activeDelay);
        spawnedIn = true;
        StartCoroutine(enemyAttackTimer());
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

    Vector3 getTargetPos()
    {
        switch (selectedAttackTarget)
        {
            case (AttackTarget.player): return player.transform.position;
            case (AttackTarget.topleft): return new Vector3(-4.44375f, 2.5f,0);
            case (AttackTarget.top): return new Vector3(0, 2.5f, 0);
            case (AttackTarget.topright): return new Vector3(4.44375f, 2.5f, 0);
            case (AttackTarget.centerleft): return new Vector3(-4.44375f, 0, 0);
            case (AttackTarget.center): return new Vector3(0, 0, 0);
            case (AttackTarget.centerright): return new Vector3(4.44375f, 0, 0);
            case (AttackTarget.bottomleft): return new Vector3(-4.44375f, -2.5f, 0);
            case (AttackTarget.bottom): return new Vector3(0, -2.5f, 0);
            case (AttackTarget.bottomright): return new Vector3(4.44375f, -2.5f, 0);
            default: return player.transform.position;
        }
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
                selectedAttackTarget = attackTargets[currPatternIndex];
                break;
            case (CycleMode.random): //Select random with equal likelihoods
                int _num = UnityEngine.Random.Range(0, numAttackPatterns);
                selectedAttackPattern = attackPatterns[_num];
                selectedAttackTarget = attackTargets[_num];
                break;
            case (CycleMode.weightedRandom): //Random with some options more likely than others
                float _rand = UnityEngine.Random.Range(0f, 1f);
                float _prvPercent = 0;
                for(int i=0; i<attackPatternWeights.Length; i++)
                {
                    if (attackPatternWeights[i] + _prvPercent > _rand) {
                        selectedAttackPattern = attackPatterns[i];
                        selectedAttackTarget = attackTargets[i];
                        break;
                    }
                    _prvPercent += attackPatternWeights[i];
                }
                break;
        }
    }
    private void OnTriggerEnter2D(Collider2D other) //I made this 2D because it would collide with the bullets otherwise
    {
        //collision with player projectiles (account for 3d) -> take damage
        if(other.CompareTag("PlayerBullet"))
        {
            TakeDamage(1);
        }
        else if(other.CompareTag("PlayerBulletCharge"))
        {
            TakeDamage(5);
        }
    }

}
