using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ReticleScript : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Equal to the X and Y Scale of a rectangle sprite that covers the whole screen")]
    private float GameScreenWidth=17.775f,GameScreenHeight=10;

    [Tooltip("ID for sfx, or index of sfx in the SoundManager sfxs array")]
    public int attackSfx,chargeAttackSfx;

    //Bullet Particle Systems
    public GameObject playerBulletStandard;
    public GameObject playerBulletCharge;

    //Shooting bools
    //bool isShooting;
    bool canShootStandard = true;
    bool canShootCharge = true;

    //Anim
    Animator anim;

    //SoundManager
    SoundManager sm;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        anim = GetComponent<Animator>();
        sm = GameObject.Find("SoundManager").GetComponent<SoundManager>();
    }

    public void PlayerLook(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Vector2 mousePixelPos = Mouse.current.position.ReadValue(); //(0,0) = bottom left corner of scrn
            mousePixelPos = new Vector2(mousePixelPos.x-(Screen.width / 2),mousePixelPos.y-(Screen.height / 2)); //(0,0) is center
            //Convert from pixels to Unity's (approx) Units
            transform.position = new Vector2((mousePixelPos.x / Screen.width) * GameScreenWidth, (mousePixelPos.y / Screen.height) * GameScreenHeight);
        }
    }

    public void PlayerFire(InputAction.CallbackContext ctx)
    {
        if (canShootStandard)
        {
            sm.PlaySFX(attackSfx,UnityEngine.Random.Range(0.9f,1.15f));
            StartCoroutine(ShootStandard());
        }
    }

    public void PlayerChargeShot(InputAction.CallbackContext ctx)
    {
        if (canShootCharge)
        {
            sm.PlaySFX(chargeAttackSfx,UnityEngine.Random.Range(0.95f, 1.05f));
            StartCoroutine(ShootCharge());
        }
    }

    IEnumerator ShootStandard()
    {
        //isShooting = true;
        canShootStandard = false;
        //instantiate bullet
        Instantiate(playerBulletStandard, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(0.25f);
        //isShooting = false;
        canShootStandard = true;
    }

    IEnumerator ShootCharge()
    {
        //isShooting = true;
        canShootCharge = false;
        anim.SetBool("canShootCharge", canShootCharge);
        //instantiate charge bullet
        Instantiate(playerBulletCharge, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(3);
        //isShooting = false;
        canShootCharge = true;
        anim.SetBool("canShootCharge", canShootCharge);
    }
}
