using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBulletScript : MonoBehaviour
{

    //INHERITED VARS
    [NonSerialized]
    public Vector3 velocity;
    [NonSerialized]
    public int damage;
    [NonSerialized]
    public bool homingBullet=false;
    [NonSerialized]
    public float speed;



    //PRIVATE VARS
    SpriteRenderer spriteRend;
    ScaleBasedOnDepth scaleDepth;
    GameObject player;
    int playerSortOrder;


    // Start is called before the first frame update
    void Start()
    {
        spriteRend = GetComponent<SpriteRenderer>();
        scaleDepth = GetComponent<ScaleBasedOnDepth>();
        spriteRend.sortingOrder = (int)scaleDepth.zpos;
        if (homingBullet) {
            player = GameObject.Find("Player");
            if (player == null) player = GameObject.Find("playerPlacehold");
            playerSortOrder = player.GetComponent<SpriteRenderer>().sortingOrder;
        }
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
        //Move x-y
        transform.Translate(velocity.x*Time.deltaTime, velocity.y * Time.deltaTime, 0);
        //Move depth
        scaleDepth.zpos += velocity.z * Time.deltaTime;

        //Despawn
        if (scaleDepth.zpos > 1) Destroy(gameObject);

        //Can hit player
        if(spriteRend.sortingOrder==0) GetComponent<Collider2D>().enabled = true;
        if(spriteRend.sortingOrder==1) GetComponent<Collider2D>().enabled = false;

    }
}
