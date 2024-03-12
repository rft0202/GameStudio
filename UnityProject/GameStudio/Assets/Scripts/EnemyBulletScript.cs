using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBulletScript : MonoBehaviour
{
    public bool spins = false;
    public float spinSpd = 1;

    //INHERITED VARS
    [NonSerialized]
    public Vector3 velocity;
    [NonSerialized]
    public int damage;
    [NonSerialized]
    public bool homingBullet=false;
    [NonSerialized]
    public float speed;
    [NonSerialized]
    public bool hurtActive = false;


    //PRIVATE VARS
    public SpriteRenderer spriteRend;
    ScaleBasedOnDepth scaleDepth;
    GameObject player;
    int playerSortOrder;
    bool destroyed = false;


    // Start is called before the first frame update
    void Start()
    {
        if (spriteRend == null)
            spriteRend = GetComponent<SpriteRenderer>();
        if (spriteRend==null)
            spriteRend = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        scaleDepth = GetComponent<ScaleBasedOnDepth>();
        spriteRend.sortingOrder = (int)scaleDepth.zpos;
        gameObject.tag = "Untagged";
        if (homingBullet) {
            player = GameObject.Find("Player");
            if (player == null) player = GameObject.Find("playerPlacehold");
            playerSortOrder = player.GetComponent<SpriteRenderer>().sortingOrder;
        }
        GetComponent<Collider2D>().enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (homingBullet) //If homing bullet, recalculate velocity
        {
            //Calculate velocity using projectileSpeed and trig and stuff

            //Preparing values for calculations
            Vector3 _sPos = transform.position;
            Vector3 playerPos = player.transform.position;
            _sPos.z = scaleDepth.zpos;
            int targetDepth = playerSortOrder+2;

            //Getting distances on x,y,z
            Vector3 dists = new(playerPos.x - _sPos.x, playerPos.y - _sPos.y, targetDepth - _sPos.z);
            //Getting total distance / magnitude
            float totalDist = Mathf.Sqrt(Mathf.Pow(dists.x, 2) + Mathf.Pow(dists.y, 2) + Mathf.Pow(dists.z, 2));
            //Scaling triangle down so hypotenuse=1, which gives the cos(angle).etc. values
            Vector3 trigVals = dists / totalDist;
            //Velocity = proportional trig values * absolute speed
            velocity = speed * trigVals;
        }
        //spin
        if (spins)
        {
            spriteRend.transform.rotation = 
            Quaternion.Euler(spriteRend.transform.rotation.eulerAngles+(spinSpd * Time.deltaTime * Vector3.forward));
        }
        //Move x-y
        transform.Translate(velocity.x*Time.deltaTime, velocity.y * Time.deltaTime, 0);
        //Move depth
        scaleDepth.zpos += velocity.z * Time.deltaTime;

        //Can hit player
        if (scaleDepth.zpos >= 0f && gameObject.CompareTag("Untagged")) StartCoroutine(updateCollision()); 

        //Despawn
        if (scaleDepth.zpos > 1) Destroy(gameObject);

    }
    private void OnTriggerEnter2D(Collider2D other) //I made this 2D because it would collide with the bullets otherwise
    {
        if (!destroyed)
        {
            //collision with player projectiles (account for 3d) -> take damage
            if (other.CompareTag("PlayerBullet"))
            {

                StartCoroutine(destroySelf(other.gameObject)); //normal bullet gets destroyed when hit bullet
            }
            else if (other.CompareTag("PlayerBulletCharge"))
            {
                StartCoroutine(destroySelf()); //charge bullet persists and destroys the projectile
            }
        }
    }

    IEnumerator destroySelf(GameObject _bullet=null)
    {
        destroyed = true;
        if (_bullet != null) _bullet.tag = "Untagged";
        yield return new WaitForSeconds(getTimeTilCollision());
        GameObject.Find("GameManager").GetComponent<GameManager>().AddToActiveScore(5, new Vector2(transform.position.x + 1, transform.position.y + 1));
        if (_bullet != null) Destroy(_bullet);
        Destroy(gameObject);
    }

    float getTimeTilCollision()
    {
        return (scaleDepth.zpos*-.025f);
    }

    IEnumerator updateCollision()
    {
        gameObject.tag = "EnemyBullet";
        GetComponent<Collider2D>().enabled = false;
        yield return null;
        GetComponent<Collider2D>().enabled = true;
    }

}
