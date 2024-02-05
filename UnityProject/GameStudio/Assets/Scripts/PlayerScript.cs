using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerScript : MonoBehaviour
{
    //Public Attributes
    [Header("Movement")]
    public float acceleration;
    public float maxSpd, dodgeSpdMult, friction;
    [Header("Timing / Cooldowns")]
    public float dodgeLength;
    public float dodgeCooldownTime, invincibilityTime;
    [Header("Other")]
    public int maxHealth;

    //Health Vars
    public GameObject[] hearts;
    public GameObject heartExplode;

    //Private Attributes
    bool isDodging = false, canDodge = true, canTakeDamage = true;
    float maxDodgeSpd;
    int health;
    Vector2 spd, inp;

    //Anim
    Animator anim;

    //SoundManager
    SoundManager sm;

    // Start is called before the first frame update
    void Start()
    {
        maxDodgeSpd = maxSpd * dodgeSpdMult; //Getting maxDodgeSpd based on the Dodge speed multiplier
        health = maxHealth;
        anim = GetComponent<Animator>();
        sm = GameObject.Find("SoundManager").GetComponent<SoundManager>();
    }

    // Update is called once per frame
    void Update()
    {
        //-----Movement-----
        spd += (inp*acceleration); //Adding acceleration to the velocity
        if (isDodging) spd *= dodgeSpdMult; //If dodging, go faster
        spd *= friction; //Applying friction to velocity
        spd = Vector2.ClampMagnitude(spd, (isDodging) ? (maxDodgeSpd) : (maxSpd)); //Clamping to maximum speed
        transform.Translate(spd*Time.deltaTime); //Moving and applying Time.deltaTime

    }

    public void PlayerMove(InputAction.CallbackContext ctx)
    {
        inp = ctx.ReadValue<Vector2>();
    }

    public void PlayerTrick(InputAction.CallbackContext ctx)
    {
        //anim
        anim.SetTrigger("performTrick");
    }

    public void PlayerDodge(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && canDodge)
        {
            isDodging = true; //Player now dodging, and cannot dodge while already dodging
            canDodge = false;
            sm.PlaySFX(5, UnityEngine.Random.Range(0.9f, 1.15f));
            StartCoroutine(EndDodge());
        }
    }

    IEnumerator EndDodge()
    {
        yield return new WaitForSeconds(dodgeLength); //After dodgeLength time, stop dodging
        isDodging = false;
        StartCoroutine(DodgeCooldown()); //When dodge finished, start cooldown
    }

    IEnumerator DodgeCooldown()
    {
        yield return new WaitForSeconds(dodgeCooldownTime); //When cooldown is done, you may dodge again
        canDodge = true;
    }

    IEnumerator DmgCooldown()
    {
        canTakeDamage = false;
        yield return new WaitForSeconds(invincibilityTime);
        canTakeDamage = true;
    }

    void TakeDamage(int dmg)
    {
        sm.PlaySFX(3, UnityEngine.Random.Range(0.9f, 1.15f));
        Instantiate(heartExplode, hearts[health - 1].transform.GetChild(0).position, Quaternion.identity);
        hearts[health-1].SetActive(false);
        health -= dmg;
        if(health<=0) PlayerDie();
    }

    void PlayerDie()
    {
        health = 0;
        //temp way of killing player
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        StartCoroutine(changeSceneDelay("GameOver", 1f));
    }

    IEnumerator changeSceneDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        Cursor.visible = true;
        SceneManager.LoadScene(sceneName);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isDodging) //Player invincible while dodging (feel free to change)
        {
            if (collision.gameObject.CompareTag("EnemyBullet"))
            {
                if (canTakeDamage) { 
                    TakeDamage(collision.gameObject.GetComponent<EnemyBulletScript>().damage);
                    StartCoroutine(DmgCooldown());
                }
                Destroy(collision.gameObject);
            }else if (collision.gameObject.CompareTag("HealthPickup"))
            {
                //health += however idk, property on pickup maybe
            }
        }
    }
}
