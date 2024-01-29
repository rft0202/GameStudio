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
    public float damage;
    [NonSerialized]
    public bool homingBullet;


    //PRIVATE VARS
    SpriteRenderer spriteRend;
    ScaleBasedOnDepth scaleDepth;


    // Start is called before the first frame update
    void Start()
    {
        spriteRend = GetComponent<SpriteRenderer>();
        scaleDepth = GetComponent<ScaleBasedOnDepth>();
        spriteRend.sortingOrder = (int)scaleDepth.zpos;
    }

    // Update is called once per frame
    void Update()
    {
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
