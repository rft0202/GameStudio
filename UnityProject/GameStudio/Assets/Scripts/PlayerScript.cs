using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    [Header("Movement")]
    public float acceleration;
    public float maxSpd, dodgeSpdMult, friction;
    [Header("Timing / Cooldowns")]
    public float dodgeCooldownTime;
    bool isDodging = false;
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
        spd *= friction; //Applying friction to velocity
        Vector2.ClampMagnitude(spd, (isDodging) ? (maxSpd) : (maxDodgeSpd)); //Clamping to maximum speed
        transform.Translate(spd*Time.deltaTime); //Moving and applying Time.deltaTime

    }

    public void PlayerMove(InputAction.CallbackContext ctx)
    {
        inp = ctx.ReadValue<Vector2>();
    }

    public void PlayerDodge(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            isDodging = true;
            StartCoroutine(DodgeCooldown());
        }
    }

    IEnumerator DodgeCooldown()
    {
        yield return new WaitForSeconds(dodgeCooldownTime);
        isDodging = false;
    }
}
