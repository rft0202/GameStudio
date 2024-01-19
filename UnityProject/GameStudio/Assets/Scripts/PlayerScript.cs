using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    //Public Attributes
    [Header("Movement")]
    public float acceleration;
    public float maxSpd, dodgeSpdMult, friction;
    [Header("Timing / Cooldowns")]
    public float dodgeLength;
    public float dodgeCooldownTime;

    //Private Attributes
    bool isDodging = false, canDodge=true;
    float maxDodgeSpd;
    Vector2 spd, inp;

    // Start is called before the first frame update
    void Start()
    {
        maxDodgeSpd = maxSpd * dodgeSpdMult; //Getting maxDodgeSpd based on the Dodge speed multiplier
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

    public void PlayerDodge(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && canDodge)
        {
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
}
