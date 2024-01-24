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
    public float zpos;
    [NonSerialized]
    public float damage;

    //PRIVATE VARS
    float depthViewDistRate=1.15f;
    SpriteRenderer spriteRend;


    // Start is called before the first frame update
    void Start()
    {
        spriteRend = GetComponent<SpriteRenderer>();
        spriteRend.sortingOrder = (int)zpos;
    }

    // Update is called once per frame
    void Update()
    {
        //Move x-y
        transform.Translate(velocity.x*Time.deltaTime, velocity.y * Time.deltaTime, 0);
        //Move depth
        zpos += velocity.z * Time.deltaTime;
        spriteRend.sortingOrder = (int)zpos;
        //Scale
        transform.localScale = Mathf.Pow(depthViewDistRate, zpos) * Vector3.one;

        //Despawn
        if (zpos > 1) Destroy(gameObject);

        //Can hit player
        if(spriteRend.sortingOrder==0) GetComponent<Collider2D>().enabled = true;
        if(spriteRend.sortingOrder==1) GetComponent<Collider2D>().enabled = false;

    }
}
