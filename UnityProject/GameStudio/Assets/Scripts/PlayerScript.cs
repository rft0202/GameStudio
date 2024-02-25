using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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
    [Header("Health")]
    public int maxHealth;

    //Health Vars
    public GameObject[] hearts;
    public GameObject heartExplode;

    public GameObject[] heartPos;

    [Header("Misc")]
    public int trickScoreAmt;
    public GameObject mainCam;

    [Header("Sfx")]
    [Tooltip("ID for sfx, or index of sfx in the SoundManager sfxs array")]
    public int dodgeSfx;
    [Tooltip("ID for sfx, or index of sfx in the SoundManager sfxs array")]
    public int hurtSfx;

    //Private Attributes
    bool isDodging = false, canDodge = true, canTakeDamage = true;
    float maxDodgeSpd;
    [NonSerialized]
    public int health;
    Vector2 spd, inp;
    bool performingTrick = false;

    //Anim
    Animator anim;
    Animator camAnim;

    SoundManager sm;
    GameManager gm;

    // Start is called before the first frame update
    void Start()
    {
        maxDodgeSpd = maxSpd * dodgeSpdMult; //Getting maxDodgeSpd based on the Dodge speed multiplier
        health = maxHealth;
        anim = GetComponent<Animator>();
        sm = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (mainCam == null) mainCam = GameObject.Find("Main Camera");
        camAnim = mainCam.GetComponent<Animator>();
        camAnim.updateMode = AnimatorUpdateMode.UnscaledTime; //cam not effected by time stop
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
        if(ctx.performed)
        {
            if(!performingTrick)
                StartCoroutine(PerformTrick(maxSpd));
        }    
    }

    IEnumerator PerformTrick(float tMaxSpd)
    {
        performingTrick = true;
        canDodge = false;
        gm.AddToActiveScore(trickScoreAmt, new Vector2(transform.position.x + 1.25f, transform.position.y + 0.8f));
        anim.SetTrigger("performTrick");
        maxSpd = 0;
        yield return new WaitForSeconds(1.0f);
        maxSpd = tMaxSpd;
        performingTrick = false;
        canDodge = true;
    }

    public void PlayerDodge(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && canDodge)
        {

            sm.PlaySFX(dodgeSfx, UnityEngine.Random.Range(0.9f, 1.15f));
            isDodging = true; //Player now dodging, and cannot dodge while already dodging
            canDodge = false;
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
        sm.PlaySFX(hurtSfx, UnityEngine.Random.Range(0.9f, 1.15f));
        anim.SetTrigger("damaged");
        Instantiate(heartExplode, heartPos[health - 1].transform.position, Quaternion.identity);
        //Instantiate(heartExplode, hearts[health - 1].transform.GetChild(0).position, Quaternion.identity);
        //Instantiate(heartExplode, hearts[health - 1].transform.position, Quaternion.identity);
        //Also spawn particles on player
        Instantiate(heartExplode, transform.position, Quaternion.identity);
        //Camera shake
        camAnim.SetTrigger("ScreenShake");
        //Time stop
        StartCoroutine(timeStop());

        hearts[health-1].SetActive(false);
        health -= dmg;
        if(health<=0) PlayerDie();
    }

    void PlayerDie()
    {
        health = 0;
        anim.SetTrigger("dying");
        gameObject.GetComponent<Rigidbody2D>().gravityScale = 2;
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
        //temp way of killing player
        //gameObject.GetComponent<SpriteRenderer>().enabled = false;
        StartCoroutine(changeSceneDelay("GameOver", 1f));
    }

    IEnumerator changeSceneDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (sceneName == "GameOver")
        {
            sm.PlaySFX(7);
        }
        Cursor.visible = true;
        SceneManager.LoadScene(sceneName);
    }

    IEnumerator timeStop()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(.25f);
        Time.timeScale = 1;
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
